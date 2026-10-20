using Application.Interfaces;
using MediatR;
using ResponseLibrary.Wrappers.Interfaces;

namespace Application.Features.Roles.Commands
{
    public class DeleteRoleCommand : IRequest<IResponseWrapper>
    {
        public string RoleId { get; set; }
    }

    public class DeleteRoleCommandHandler(IRoleService roleService) : IRequestHandler<DeleteRoleCommand, IResponseWrapper>
    {
        private readonly IRoleService _roleService = roleService;

        public async Task<IResponseWrapper> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
        {
            return await _roleService.DeleteRoleAsync(request.RoleId);
        }
    }
}
