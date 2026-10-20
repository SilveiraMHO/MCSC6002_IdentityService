using Application.Interfaces;
using MediatR;
using ResponseLibrary.Wrappers.Interfaces;

namespace Application.Features.Roles.Queries
{
    public class GetPermissionsQuery : IRequest<IResponseWrapper>
    {
        public string RoleId { get; set; }
    }

    public class GetPermissionQueryHandler(IRoleService roleService) : IRequestHandler<GetPermissionsQuery, IResponseWrapper>
    {
        private readonly IRoleService _roleService = roleService;

        public async Task<IResponseWrapper> Handle(GetPermissionsQuery request, CancellationToken cancellationToken)
        {
            return await _roleService.GetPermissionAsync(request.RoleId);
        }
    }
}
