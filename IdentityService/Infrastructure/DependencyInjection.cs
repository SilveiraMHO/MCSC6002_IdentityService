using Application.Interfaces;
using Infrastructure.Context;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration config)
        {
            return services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(config.GetConnectionString("DefaultConnection"), builder =>
                {
                    builder.MigrationsHistoryTable("Migrations", "EFCore");
                    builder.EnableRetryOnFailure(maxRetryCount: 3,
                        maxRetryDelay: new TimeSpan(0, 0, 0, 100),
                        errorNumbersToAdd: new[] { 1 });
                }))
                .AddScoped<ApplicationDbSeeder>();
        }

        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            return services
                .AddScoped<IUserService, UserService>()
                .AddScoped<IRoleService, RoleService>()
                .AddScoped<ITokenService, TokenService>();
        }
    }
}
