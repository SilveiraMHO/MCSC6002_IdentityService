using AuthLibrary.Constants.Authentication;
using Infrastructure.Constants;
using Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Context
{
    public class ApplicationDbSeeder
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public ApplicationDbSeeder(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, ApplicationDbContext context, IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _configuration = configuration;
        }

        public async Task SeedIdentityDatabaseAsync()
        {
            await CheckAndApplyPendingMigrationAsync();
            await SeedRolesAsync();
            await SeedAdminUserAsync();
            await SeedBasicUserAsync();
        }

        private async Task CheckAndApplyPendingMigrationAsync()
        {
            if (_context.Database.GetPendingMigrations().Any())
            {
                await _context.Database.MigrateAsync();
            }
        }

        private async Task SeedAdminUserAsync()
        {
            string adminEmail = _configuration["SeedCredentials:AdminEmail"];
            string adminPassword = _configuration["SeedCredentials:AdminPassword"];

            var user = new ApplicationUser
            {
                FirstName = "admin",
                LastName = "admin",
                Email = adminEmail,
                UserName = adminEmail,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                PhoneNumber = "9999 9999",
                NormalizedEmail = adminEmail.ToUpperInvariant(),
                NormalizedUserName = adminEmail.ToUpperInvariant(),
                IsActive = true
            };

            if (!await _userManager.Users.AnyAsync(u => u.Email == adminEmail))
            {
                var password = new PasswordHasher<ApplicationUser>();
                user.PasswordHash = password.HashPassword(user, adminPassword);
                await _userManager.CreateAsync(user);
            }

            user = await _userManager.FindByEmailAsync(adminEmail);

            // Assign role(s) to user
            if (!await _userManager.IsInRoleAsync(user, AppRoles.Basic) && !await _userManager.IsInRoleAsync(user, AppRoles.Admin))
            {
                await _userManager.AddToRolesAsync(user, AppRoles.DefaultRoles);
            }
        }

        private async Task SeedBasicUserAsync()
        {
            string basicEmail = _configuration["SeedCredentials:BasicEmail"];
            string basicPassword = _configuration["SeedCredentials:BasicPassword"];

            var user = new ApplicationUser
            {
                FirstName = "basic",
                LastName = "basic",
                Email = basicEmail,
                UserName = basicEmail,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                PhoneNumber = "8888 8888",
                NormalizedEmail = basicEmail.ToUpperInvariant(),
                NormalizedUserName = basicEmail.ToUpperInvariant(),
                IsActive = true
            };

            if (!await _userManager.Users.AnyAsync(u => u.Email == basicEmail))
            {
                var password = new PasswordHasher<ApplicationUser>();
                user.PasswordHash = password.HashPassword(user, basicPassword);
                await _userManager.CreateAsync(user);
            }

            user = await _userManager.FindByEmailAsync(basicEmail);

            // Assign role(s) to user
            if (!await _userManager.IsInRoleAsync(user, AppRoles.Basic))
            {
                await _userManager.AddToRoleAsync(user, AppRoles.Basic);
            }
        }

        private async Task SeedRolesAsync()
        {
            foreach (var roleName in AppRoles.DefaultRoles)
            {
                if (await _roleManager.Roles.FirstOrDefaultAsync(r => r.Name == roleName) is not ApplicationRole role)
                {
                    role = new ApplicationRole
                    {
                        Name = roleName,
                        Description = $"{roleName} Role.",
                        NormalizedName = roleName.ToUpperInvariant()
                    };

                    await _roleManager.CreateAsync(role);
                }

                // Assign permissions to role.
                if (roleName == AppRoles.Basic)
                {
                    await AssignPermissionsToRoleAsync(role, AppPermissions.BasicPermissions);
                }
                else if (roleName == AppRoles.Admin)
                {
                    await AssignPermissionsToRoleAsync(role, AppPermissions.AdminPermissions);
                }
            }
        }

        private async Task AssignPermissionsToRoleAsync(ApplicationRole role, IReadOnlyList<AppPermission> permissions)
        {
            var currentAssignedClaims = await _roleManager.GetClaimsAsync(role);

            foreach (var permission in permissions)
            {
                if (!currentAssignedClaims.Any(claim => claim.Type == AppClaim.Permission && claim.Value == permission.Name))
                {
                    await _context.RoleClaims.AddAsync(new ApplicationRoleClaim
                    {
                        RoleId = role.Id,
                        ClaimType = AppClaim.Permission,
                        ClaimValue = permission.Name,
                        Description = permission.Description,
                        Group = permission.Group
                    });

                    await _context.SaveChangesAsync();
                }
            }
        }
    }
}
