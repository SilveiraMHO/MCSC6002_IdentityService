using Application.Interfaces;
using AuthLibrary.Constants.Authentication;
using Infrastructure.Constants;
using Infrastructure.Context;
using Infrastructure.Helper;
using Infrastructure.Models;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ResponseLibrary.Models.Requests.Identity;
using ResponseLibrary.Models.Responses.Identity;
using ResponseLibrary.Models.Responses.Identity.ViewModelHelper;
using ResponseLibrary.Wrappers;
using ResponseLibrary.Wrappers.Interfaces;

namespace Infrastructure.Services
{
    public class RoleService : IRoleService
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public RoleService(RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _context = context;
        }

        public async Task<IResponseWrapper> CreateRoleAsync(CreateRoleRequest roleRegistrationRequest)
        {
            var roleDb = await _roleManager.FindByIdAsync(roleRegistrationRequest.Name);

            if (roleDb is not null)
            {
                return await ResponseWrapper.FailAsync("Role already exists.");
            }

            var newRole = new ApplicationRole
            {
                Name = roleRegistrationRequest.Name,
                Description = roleRegistrationRequest.Description
            };

            var result = await _roleManager.CreateAsync(newRole);

            if (result.Succeeded)
            {
                return await ResponseWrapper.SuccessAsync("Role created successfully.");
            }

            return await ResponseWrapper.FailAsync(IdentityHelper.GetIdentityResultErrorDescriptions(result));
        }

        public async Task<IResponseWrapper> DeleteRoleAsync(string roleId)
        {
            var roleDb = await _roleManager.FindByIdAsync(roleId);

            if (roleDb is not null)
            {
                // Admin role can't be deleted.
                if (roleDb.Name != AppRoles.Admin)
                {
                    var allUsersDb = await _userManager.Users.ToListAsync();

                    foreach (var user in allUsersDb)
                    {
                        if (await _userManager.IsInRoleAsync(user, roleDb.Name))
                        {
                            return await ResponseWrapper.FailAsync($"Cannot delete the role: {roleDb.Name}. There are users assigned to this role.");
                        }
                    }

                    var identityResult = await _roleManager.DeleteAsync(roleDb);

                    if (identityResult.Succeeded)
                    {
                        return await ResponseWrapper.SuccessAsync("Role deleted successfully.");
                    }

                    return await ResponseWrapper.FailAsync(IdentityHelper.GetIdentityResultErrorDescriptions(identityResult));
                }

                return await ResponseWrapper.FailAsync("Cannot delete the Admin role.");
            }

            return await ResponseWrapper.FailAsync("Role not found.");
        }

        public async Task<IResponseWrapper> GetPermissionAsync(string roleId)
        {
            var roleDb = await _roleManager.FindByIdAsync(roleId);

            if (roleDb is not null)
            {
                var allPermissions = AppPermissions.AllPermissions;

                var roleClaimResponse = new RoleClaimResponse
                {
                    Role = new RoleResponse
                    {
                        Id = roleDb.Id,
                        Name = roleDb.Name,
                        Description = roleDb.Description
                    },
                    RoleClaims = new List<RoleClaimViewModel>()
                };

                var currentAssignedClaims = await GetAllClaimsForRoleAsync(roleId);

                var allPermissionNames = allPermissions.Select(p => p.Name).ToList();

                var currentAssignedClaimValues = currentAssignedClaims.Select(rc => rc.ClaimValue).ToList();

                var currentAssignedRoleClaimsNames = allPermissionNames.Intersect(currentAssignedClaimValues).ToList();

                foreach (var permission in allPermissions)
                {
                    if (currentAssignedRoleClaimsNames.Any(carc => carc == permission.Name))
                    {
                        roleClaimResponse.RoleClaims.Add(new RoleClaimViewModel
                        {
                            RoleId = roleId,
                            ClaimType = AppClaim.Permission,
                            ClaimValue = permission.Name,
                            Description = permission.Description,
                            Group = permission.Group,
                            IsAssignedRole = true
                        });
                    }
                    else
                    {
                        roleClaimResponse.RoleClaims.Add(new RoleClaimViewModel
                        {
                            RoleId = roleId,
                            ClaimType = AppClaim.Permission,
                            ClaimValue = permission.Name,
                            Description = permission.Description,
                            Group = permission.Group,
                            IsAssignedRole = false
                        });
                    }
                }

                return await ResponseWrapper<RoleClaimResponse>.SuccessAsync(roleClaimResponse);
            }

            return await ResponseWrapper.FailAsync("Role not found.");
        }

        public async Task<IResponseWrapper> GetAllRolesAsync()
        {
            var allRolesDb = await _roleManager.Roles.ToListAsync();

            if (allRolesDb.Count > 0)
            {
                var mappedRoles = allRolesDb.Adapt<List<RoleResponse>>();
                return await ResponseWrapper<List<RoleResponse>>.SuccessAsync(mappedRoles);
            }

            return await ResponseWrapper.FailAsync("No roles found.");
        }

        public async Task<IResponseWrapper> GetRoleByIdAsync(string roleId)
        {
            var roleDb = await _roleManager.FindByIdAsync(roleId);

            if (roleDb is not null)
            {
                var mappedRole = roleDb.Adapt<RoleResponse>();
                return await ResponseWrapper<RoleResponse>.SuccessAsync(mappedRole);
            }

            return await ResponseWrapper.FailAsync("Role not found.");
        }

        public async Task<IResponseWrapper> UpdateRoleAsync(UpdateRoleRequest roleUpdateRequest)
        {
            var roleDb = await _roleManager.FindByIdAsync(roleUpdateRequest.RoleId);

            if (roleDb is not null)
            {
                if (roleDb.Name == AppRoles.Admin)
                {
                    roleDb.Name = roleUpdateRequest.Name;
                    roleDb.Description = roleUpdateRequest.Description;

                    var identityResult = await _roleManager.UpdateAsync(roleDb);

                    if (identityResult.Succeeded)
                    {
                        return await ResponseWrapper.SuccessAsync("Role updated successfully.");
                    }

                    return await ResponseWrapper.FailAsync(IdentityHelper.GetIdentityResultErrorDescriptions(identityResult));
                }

                return await ResponseWrapper.FailAsync("Cannot update the Admin role.");
            }

            return await ResponseWrapper.FailAsync("Role not found.");
        }

        public async Task<IResponseWrapper> UpdateRolePermissionAsync(UpdateRoleClaimsRequest roleClaimsUpdateRequest)
        {
            var roleDb = await _roleManager.FindByIdAsync(roleClaimsUpdateRequest.RoleId);

            if (roleDb is not null)
            {
                if (roleDb.Name == AppRoles.Admin)
                {
                    return await ResponseWrapper.FailAsync("Cannot update permissions for the Admin role.");
                }

                var permissionsToBeAssigned = roleClaimsUpdateRequest.RoleClaims.Where(rc => rc.IsAssignedRole).ToList();

                var currentAssignedPermissions = await _roleManager.GetClaimsAsync(roleDb);

                foreach (var claim in currentAssignedPermissions)
                {
                    await _roleManager.RemoveClaimAsync(roleDb, claim);
                }

                var mappedRoleClaims = permissionsToBeAssigned.Adapt<List<ApplicationRoleClaim>>();
                await _context.RoleClaims.AddRangeAsync(mappedRoleClaims);
                await _context.SaveChangesAsync();

                return await ResponseWrapper.SuccessAsync("Role permissions updated successfully.");
            }

            return await ResponseWrapper.FailAsync("Role not found.");
        }

        #region Private Methods
        private async Task<List<RoleClaimViewModel>> GetAllClaimsForRoleAsync(string roleId)
        {
            var roleClaims = await _context.RoleClaims.Where(rc => rc.RoleId == roleId).ToListAsync();

            if (roleClaims.Count > 0)
            {
                var mappedRoleClaims = roleClaims.Adapt<List<RoleClaimViewModel>>();
                return mappedRoleClaims;
            }

            return new List<RoleClaimViewModel>();
        }
        #endregion
    }
}
