using Application.Interfaces;
using Infrastructure.Constants;
using Infrastructure.Helper;
using Infrastructure.Models;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ResponseLibrary.Models.Requests.Identity;
using ResponseLibrary.Models.Responses.Identity;
using ResponseLibrary.Models.Responses.Identity.ViewModelHelper;
using ResponseLibrary.Wrappers;
using ResponseLibrary.Wrappers.Interfaces;

namespace Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManage;
        private readonly RoleManager<ApplicationRole> _roleManage;
        private readonly IConfiguration _configuration;

        public UserService(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, IConfiguration configuration)
        {
            _userManage = userManager;
            _roleManage = roleManager;
            _configuration = configuration;
        }

        public async Task<IResponseWrapper> ChangeUserPasswordAsync(ChangeUserPasswordRequest userPasswordChange)
        {
            var existentUser = await _userManage.FindByIdAsync(userPasswordChange.UserId);

            if (existentUser is not null)
            {
                var identityResult = await _userManage.ChangePasswordAsync(existentUser, userPasswordChange.CurrentPassword, userPasswordChange.NewPassword);

                if (identityResult.Succeeded)
                {
                    return await ResponseWrapper.SuccessAsync("Password changed successfully.");
                }

                return await ResponseWrapper.FailAsync(IdentityHelper.GetIdentityResultErrorDescriptions(identityResult));
            }

            return await ResponseWrapper.FailAsync("User not found.");
        }

        public async Task<IResponseWrapper> ChangeUserStatusAsync(ChangeUserStatusRequest userChangeStatus)
        {
            var existentUser = await _userManage.FindByIdAsync(userChangeStatus.UserId);

            if (existentUser is not null)
            {
                existentUser.IsActive = userChangeStatus.NewIsActiveStatus;

                var identityResult = await _userManage.UpdateAsync(existentUser);

                if (identityResult.Succeeded)
                {
                    return await ResponseWrapper.SuccessAsync(userChangeStatus.NewIsActiveStatus ? "User activated successfully." : "User deactivated successfully.");
                }
                return await ResponseWrapper.FailAsync(IdentityHelper.GetIdentityResultErrorDescriptions(identityResult));
            }

            return await ResponseWrapper.FailAsync("User not found.");
        }

        public async Task<IResponseWrapper> GetAllUsersAsync()
        {
            var existentUsers = await _userManage.Users.ToListAsync();

            if (existentUsers.Count > 0)
            {
                var mappedUsers = existentUsers.Adapt<List<UserResponse>>();

                return await ResponseWrapper<List<UserResponse>>.SuccessAsync(mappedUsers);
            }

            return await ResponseWrapper.FailAsync("No users found.");
        }

        public async Task<IResponseWrapper> GetUserByEmailAsync(string email)
        {
            var existentUser = await _userManage.FindByEmailAsync(email);

            if (existentUser is not null)
            {
                var mappedUser = existentUser.Adapt<UserResponse>();

                return await ResponseWrapper<UserResponse>.SuccessAsync(mappedUser);
            }

            return await ResponseWrapper.FailAsync("User not found");
        }

        public async Task<IResponseWrapper> GetUserByIdAsync(string userId)
        {
            var existentUser = await _userManage.FindByIdAsync(userId);

            if (existentUser is not null)
            {
                var mappedUser = existentUser.Adapt<UserResponse>();

                return await ResponseWrapper<UserResponse>.SuccessAsync(mappedUser);
            }

            return await ResponseWrapper.FailAsync("User not found.");
        }

        public async Task<IResponseWrapper> GetUserRolesAsync(string userId)
        {
            var userRoleViewModel = new List<UserRoleViewModel>();

            var userDb = await _userManage.FindByIdAsync(userId);

            if (userDb is not null)
            {
                var AllRolesDb = await _roleManage.Roles.ToListAsync();

                foreach (var role in AllRolesDb)
                {
                    var userRole = new UserRoleViewModel
                    {
                        RoleName = role.Name,
                        RoleDescription = role.Description,
                    };

                    if (await _userManage.IsInRoleAsync(userDb, role.Name))
                    {
                        userRole.IsAssignedToUser = true;
                    }
                    else
                    {
                        userRole.IsAssignedToUser = false;
                    }

                    userRoleViewModel.Add(userRole);
                }

                return await ResponseWrapper<List<UserRoleViewModel>>.SuccessAsync(userRoleViewModel);
            }

            return await ResponseWrapper.FailAsync("User not found.");
        }

        public async Task<IResponseWrapper> CreateUserAsync(CreateUserRequest userRegistration)
        {
            var existentUserWithSameEmail = await _userManage.FindByEmailAsync(userRegistration.Email);
            if (existentUserWithSameEmail is not null) return await ResponseWrapper.FailAsync("Email is already in use.");

            var newUser = new ApplicationUser
            {
                FirstName = userRegistration.FirstName,
                LastName = userRegistration.LastName,
                Email = userRegistration.Email,
                UserName = userRegistration.UserName,
                PhoneNumber = userRegistration.PhoneNumber,
                IsActive = userRegistration.ActivateUser,
                EmailConfirmed = userRegistration.AutoConfirmEmail
            };

            // With password: Identity enforces the password policy.
            var identityUserResult = await _userManage.CreateAsync(newUser, userRegistration.Password);

            if (identityUserResult.Succeeded)
            {
                var identityRoleResult = await _userManage.AddToRoleAsync(newUser, AppRoles.Basic);

                if (identityRoleResult.Succeeded)
                {
                    return await ResponseWrapper.SuccessAsync("User registered successfully.");
                }

                return await ResponseWrapper.FailAsync(IdentityHelper.GetIdentityResultErrorDescriptions(identityRoleResult));
            }

            return await ResponseWrapper.FailAsync(IdentityHelper.GetIdentityResultErrorDescriptions(identityUserResult));
        }

        public async Task<IResponseWrapper> UpdateUserAsync(UpdateUserRequest userUpdate)
        {
            var existentUser = await _userManage.FindByIdAsync(userUpdate.UserId);

            if (existentUser is not null)
            {
                existentUser.FirstName = userUpdate.FirstName;
                existentUser.LastName = userUpdate.LastName;
                existentUser.PhoneNumber = userUpdate.PhoneNumber;

                var identityResult = await _userManage.UpdateAsync(existentUser);

                if (identityResult.Succeeded)
                {
                    return await ResponseWrapper.SuccessAsync("User updated successfully.");
                }

                return await ResponseWrapper.FailAsync(IdentityHelper.GetIdentityResultErrorDescriptions(identityResult));
            }

            return await ResponseWrapper.FailAsync("User not found.");
        }

        public async Task<IResponseWrapper> UpdateUserRolesAsync(UpdateUserRolesRequest userRolesUpdate)
        {
            var userDb = await _userManage.FindByIdAsync(userRolesUpdate.UserId);

            if (userDb is not null)
            {
                // Main admin's roles are fixed.
                if (userDb.Email == _configuration["SeedCredentials:AdminEmail"])
                {
                    return await ResponseWrapper.FailAsync("Cannot change roles of the main admin user.");
                }

                var userCurrentRoles = await _userManage.GetRolesAsync(userDb);

                var rolesToBeAssigned = userRolesUpdate.Roles.Where(role => role.IsAssignedToUser).ToList();

                var identityRemoveResult = await _userManage.RemoveFromRolesAsync(userDb, userCurrentRoles);

                if (identityRemoveResult.Succeeded)
                {
                    var identityAssignResult = await _userManage.AddToRolesAsync(userDb, rolesToBeAssigned.Select(role => role.RoleName));

                    if (identityAssignResult.Succeeded)
                    {
                        return await ResponseWrapper.SuccessAsync("User roles updated successfully.");
                    }

                    return await ResponseWrapper.FailAsync(IdentityHelper.GetIdentityResultErrorDescriptions(identityAssignResult));
                }

                return await ResponseWrapper.FailAsync(IdentityHelper.GetIdentityResultErrorDescriptions(identityRemoveResult));
            }

            return await ResponseWrapper.FailAsync("User not found.");
        }
    }
}
