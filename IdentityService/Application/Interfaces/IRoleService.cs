using ResponseLibrary.Models.Requests.Identity;
using ResponseLibrary.Wrappers.Interfaces;

namespace Application.Interfaces
{
    public interface IRoleService
    {
        Task<IResponseWrapper> CreateRoleAsync(CreateRoleRequest roleRegistrationRequest);
        Task<IResponseWrapper> UpdateRoleAsync(UpdateRoleRequest roleUpdateRequest);
        Task<IResponseWrapper> DeleteRoleAsync(string roleId);
        Task<IResponseWrapper> GetAllRolesAsync();
        Task<IResponseWrapper> GetRoleByIdAsync(string roleId);
        Task<IResponseWrapper> GetPermissionAsync(string roleId);
        Task<IResponseWrapper> UpdateRolePermissionAsync(UpdateRoleClaimsRequest roleClaimsUpdateRequest);
    }
}
