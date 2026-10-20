using Application.Interfaces;
using MediatR;
using ResponseLibrary.Wrappers.Interfaces;

namespace Application.Features.Roles.Queries
{
    public class GetAllRolesQuery : IRequest<IResponseWrapper>
    {
    }

    public class GetAllRolesQueryHandler(IRoleService roleService) : IRequestHandler<GetAllRolesQuery, IResponseWrapper>
    {
        private readonly IRoleService _roleService = roleService;

        public async Task<IResponseWrapper> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
        {
            return await _roleService.GetAllRolesAsync();
        }
    }
}
