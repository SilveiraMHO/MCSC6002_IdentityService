using Application.Interfaces;
using MediatR;
using ResponseLibrary.Models.Requests.Identity;
using ResponseLibrary.Wrappers.Interfaces;

namespace Application.Features.Roles.Commands
{
    public class UpdateRolePermissionsCommand : IRequest<IResponseWrapper>
    {
        public UpdateRoleClaimsRequest UpdateRoleClaims { get; set; }
    }

    public class UpdateRolePermissionsCommandHandler(IRoleService roleService) : IRequestHandler<UpdateRolePermissionsCommand, IResponseWrapper>
    {
        private readonly IRoleService _roleService = roleService;

        public async Task<IResponseWrapper> Handle(UpdateRolePermissionsCommand request, CancellationToken cancellationToken)
        {
            return await _roleService.UpdateRolePermissionAsync(request.UpdateRoleClaims);
        }
    }
}
