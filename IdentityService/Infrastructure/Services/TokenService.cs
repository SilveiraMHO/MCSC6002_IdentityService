using Application;
using Application.Interfaces;
using AuthLibrary.Constants.Authentication;
using Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ResponseLibrary.Models.Requests.Identity;
using ResponseLibrary.Models.Responses.Identity;
using ResponseLibrary.Wrappers;
using ResponseLibrary.Wrappers.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Services
{
    public class TokenService : ITokenService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly TokenSettings _tokenSettings;

        public TokenService(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, IOptions<TokenSettings> tokenSettings)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _tokenSettings = tokenSettings.Value;
        }

        public async Task<IResponseWrapper> GetTokenAsync(TokenRequest tokenRequest)
        {
            #region Validations
            // Check user name.
            var userDb = await _userManager.FindByNameAsync(tokenRequest.UserName);
            if (userDb is null)
            {
                return await ResponseWrapper.FailAsync("Invalid credentials.");
            }

            // Check password.
            var isPasswordValid = await _userManager.CheckPasswordAsync(userDb, tokenRequest.Password);
            if (!isPasswordValid)
            {
                return await ResponseWrapper.FailAsync("Invalid credentials.");
            }

            // Check if Active.
            if (!userDb.IsActive)
            {
                return await ResponseWrapper.FailAsync("User not active. Please, contact the Administrator.");
            }

            // Check email confirmed.
            if (!userDb.EmailConfirmed)
            {
                return await ResponseWrapper.FailAsync("Email not confirmed.");
            }
            #endregion

            userDb.RefreshToken = GenerateRefreshToken();
            userDb.RefreshTokenExpirationDate = DateTime.Now.AddDays(_tokenSettings.RefreshTokenExpireInDays);

            await _userManager.UpdateAsync(userDb);

            // Generate AuthToken.
            var token = await GenerateJwtAsync(userDb);

            var tokenResponse = new TokenResponse
            {
                Token = token,
                RefreshToken = userDb.RefreshToken,
                RefreshTokenExpireTime = userDb.RefreshTokenExpirationDate
            };

            return await ResponseWrapper<TokenResponse>.SuccessAsync(tokenResponse);
        }

        public async Task<IResponseWrapper> GetRefreshTokenAsync(RefreshTokenRequest refreshTokenRequest)
        {
            var userPrincipal = GetClaimPrincipalFromExpiredToken(refreshTokenRequest.Token);
            var userEmail = userPrincipal.FindFirstValue(ClaimTypes.Email);

            var userDb = await _userManager.FindByEmailAsync(userEmail);

            if (userDb is not null)
            {
                if (userDb.RefreshToken != refreshTokenRequest.RefreshToken || userDb.RefreshTokenExpirationDate <= DateTime.Now)
                {
                    return await ResponseWrapper.FailAsync("Invalid token provided.");
                }

                var token = GenerateEncryptedToken(GetSigningCredentials(), await GetClaimsAsync(userDb));

                userDb.RefreshToken = GenerateRefreshToken();
                userDb.RefreshTokenExpirationDate = DateTime.Now.AddDays(_tokenSettings.RefreshTokenExpireInDays);

                await _userManager.UpdateAsync(userDb);

                var tokenResponse = new TokenResponse
                {
                    Token = token,
                    RefreshToken = userDb.RefreshToken,
                    RefreshTokenExpireTime = userDb.RefreshTokenExpirationDate
                };

                return await ResponseWrapper<TokenResponse>.SuccessAsync(tokenResponse);
            }

            return await ResponseWrapper.FailAsync("User not found.");
        }

        #region Private Methods
        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private async Task<IEnumerable<Claim>> GetClaimsAsync(ApplicationUser user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);

            var roleClaims = new List<Claim>();
            var permissionClaims = new List<Claim>();

            foreach (var role in roles)
            {
                roleClaims.Add(new Claim(ClaimTypes.Role, role));
                var currentRole = await _roleManager.FindByNameAsync(role);
                var allPermissionsForCurrentRole = await _roleManager.GetClaimsAsync(currentRole);

                permissionClaims.AddRange(allPermissionsForCurrentRole);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FirstName),
                new Claim(ClaimTypes.Surname, user.LastName),
                new Claim(ClaimTypes.MobilePhone, user.PhoneNumber ?? string.Empty)
            }
            .Union(roleClaims)
            .Union(userClaims)
            .Union(permissionClaims);

            return claims;
        }

        private SigningCredentials GetSigningCredentials()
        {
            var secret = Encoding.UTF8.GetBytes(_tokenSettings.Secret);

            return new SigningCredentials(new SymmetricSecurityKey(secret), SecurityAlgorithms.HmacSha256);
        }

        private string GenerateEncryptedToken(SigningCredentials signingCredentials, IEnumerable<Claim> claims)
        {
            var token = new JwtSecurityToken(
                claims: claims,
                issuer: AppClaim.Issuer,
                audience: AppClaim.Audience,
                expires: DateTime.Now.AddMinutes(_tokenSettings.TokenExpireInMinutes),
                signingCredentials: signingCredentials);

            var tokenHandler = new JwtSecurityTokenHandler();
            var encryptedToken = tokenHandler.WriteToken(token);

            return encryptedToken;
        }

        private async Task<string> GenerateJwtAsync(ApplicationUser user)
        {
            var token = GenerateEncryptedToken(GetSigningCredentials(), await GetClaimsAsync(user));

            return token;
        }

        private ClaimsPrincipal GetClaimPrincipalFromExpiredToken(string expiredToken)
        {
            var tokenValidationParms = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidIssuer = AppClaim.Issuer,
                ValidAudience = AppClaim.Audience,
                RoleClaimType = ClaimTypes.Role,
                ClockSkew = TimeSpan.Zero,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenSettings.Secret))
            };

            var tokenHanlder = new JwtSecurityTokenHandler();

            var principal = tokenHanlder.ValidateToken(expiredToken, tokenValidationParms, out var securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid Token");
            }

            return principal;
        }
        #endregion
    }
}
