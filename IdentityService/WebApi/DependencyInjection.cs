using Application;
using AuthLibrary.Constants.Authentication;
using AuthLibrary.Permissions;
using Infrastructure.Context;
using Infrastructure.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using ResponseLibrary.Wrappers;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;

namespace WebApi
{
    internal static class DependencyInjection
    {
        internal static async Task<IApplicationBuilder> SeedDatabase(this IApplicationBuilder app)
        {
            using var serviceScope = app.ApplicationServices.CreateScope();

            var seeder = serviceScope.ServiceProvider.GetService<ApplicationDbSeeder>();

            await seeder.SeedIdentityDatabaseAsync();

            return app;
        }

        internal static IServiceCollection AddIdentitySettings(this IServiceCollection services)
        {
            services
                .AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>()
                .AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>()
                .AddIdentity<ApplicationUser, ApplicationRole>(options =>
                {
                    // NIST SP 800-63B: length over composition rules.
                    options.Password.RequiredLength = 12;
                    options.Password.RequiredUniqueChars = 6;
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                    options.User.RequireUniqueEmail = true;

                    // Per account; the gateway limits per IP.
                    options.Lockout.MaxFailedAccessAttempts = 5;
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                    options.Lockout.AllowedForNewUsers = true;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            return services;
        }

        internal static TokenSettings GetTokenSettings(this IServiceCollection services, IConfiguration config)
        {
            var tokenSettingsConfig = config.GetSection(nameof(TokenSettings));

            services.Configure<TokenSettings>(tokenSettingsConfig);

            return tokenSettingsConfig.Get<TokenSettings>();
        }

        // mTLS: only the gateway's CA-issued client cert can connect.
        internal static IWebHostBuilder AddMutualTls(this IWebHostBuilder webHost, IConfiguration config)
        {
            var settings = config.GetSection("MutualTls");
            var ca = X509CertificateLoader.LoadCertificateFromFile(settings["CaCertificatePath"]);
            var serverCertificate = X509CertificateLoader.LoadPkcs12FromFile(settings["ServerCertificatePath"], password: null);
            var allowedClientName = settings["AllowedClientName"];

            return webHost.ConfigureKestrel(kestrel => kestrel.ConfigureHttpsDefaults(https =>
            {
                https.ServerCertificate = serverCertificate;
                https.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
                https.ClientCertificateValidation = (certificate, _, _) =>
                    certificate.GetNameInfo(X509NameType.SimpleName, false) == allowedClientName
                    && IsIssuedByCa(certificate, ca);
            }));
        }

        private static bool IsIssuedByCa(X509Certificate2 certificate, X509Certificate2 ca)
        {
            using var chain = new X509Chain();
            chain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust; // Project CA only.
            chain.ChainPolicy.CustomTrustStore.Add(ca);
            chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck; // Limitation: no CRL, certs not revocable.
            chain.ChainPolicy.ApplicationPolicy.Add(new Oid("1.3.6.1.5.5.7.3.2")); // EKU: client authentication.

            return chain.Build(certificate);
        }

        internal static IServiceCollection AddJwtAuthentication(this IServiceCollection services, TokenSettings tokenSettings)
        {
            // RS256: only Identity holds the private key.
            var rsa = RSA.Create();
            rsa.ImportFromPem(File.ReadAllText(tokenSettings.PrivateKeyPath));
            var signingKey = new RsaSecurityKey(rsa);

            services.AddSingleton(signingKey); // Used by TokenService to sign.

            services
                .AddAuthentication(auth =>
                {
                    auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    auth.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(bearer =>
                {
                    bearer.RequireHttpsMetadata = false;
                    bearer.SaveToken = true;
                    bearer.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidIssuer = AppClaim.Issuer,
                        ValidAudience = AppClaim.Audience,
                        RoleClaimType = ClaimTypes.Role,
                        ClockSkew = TimeSpan.Zero,
                        IssuerSigningKey = signingKey,
                        ValidAlgorithms = [SecurityAlgorithms.RsaSha256]
                    };

                    bearer.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            if (context.Exception is SecurityTokenExpiredException)
                            {
                                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                                context.Response.ContentType = "application/json";

                                var result = JsonSerializer.Serialize(ResponseWrapper.Fail("The token is expired."));

                                return context.Response.WriteAsync(result);
                            }
                            else
                            {
                                // Invalid token: 401, not 500.
                                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                                context.Response.ContentType = "application/json";

                                var result = JsonSerializer.Serialize(ResponseWrapper.Fail("Invalid token."));

                                return context.Response.WriteAsync(result);
                            }
                        },
                        OnChallenge = context =>
                        {
                            context.HandleResponse();

                            if (!context.Response.HasStarted)
                            {
                                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                                context.Response.ContentType = "application/json";

                                var result = JsonSerializer.Serialize(ResponseWrapper.Fail("You are not Authorized."));

                                return context.Response.WriteAsync(result);
                            }

                            return Task.CompletedTask;
                        },
                        OnForbidden = context =>
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                            context.Response.ContentType = "application/json";

                            var result = JsonSerializer.Serialize(ResponseWrapper.Fail("You are not authorized to access this resource."));

                            return context.Response.WriteAsync(result);
                        }
                    };
                });

            // One policy per AppPermissions entry; no DB lookup per request.
            services.
                AddAuthorization(options =>
                {
                    foreach (var prop in typeof(AppPermissions).GetNestedTypes()
                        .SelectMany(c => c.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)))
                    {
                        var propertyValue = prop.GetValue(null);

                        if (propertyValue is not null)
                        {
                            options.AddPolicy(propertyValue.ToString(), policy => policy.RequireClaim(AppClaim.Permission, propertyValue.ToString()));
                        }
                    }
                });

            return services;
        }
    }
}
