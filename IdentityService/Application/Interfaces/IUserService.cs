using ResponseLibrary.Models.Requests.Identity;
using ResponseLibrary.Wrappers.Interfaces;

namespace Application.Interfaces
{
    public interface IUserService
    {
        Task<IResponseWrapper> CreateUserAsync(CreateUserRequest userRegistrationRequest);
        Task<IResponseWrapper> UpdateUserAsync(UpdateUserRequest userUpdateRequest);
        Task<IResponseWrapper> GetUserByIdAsync(string userId);
        Task<IResponseWrapper> GetAllUsersAsync();
        Task<IResponseWrapper> ChangeUserPasswordAsync(ChangeUserPasswordRequest userChangePasswordRequest);
        Task<IResponseWrapper> ChangeUserStatusAsync(ChangeUserStatusRequest userChangeStatusRequest);
        Task<IResponseWrapper> GetUserRolesAsync(string userId);
        Task<IResponseWrapper> UpdateUserRolesAsync(UpdateUserRolesRequest userRolesUpdateRequest);
        Task<IResponseWrapper> GetUserByEmailAsync(string email);
    }
}
