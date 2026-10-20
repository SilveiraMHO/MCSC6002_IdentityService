using Application.Interfaces;
using MediatR;
using ResponseLibrary.Models.Requests.Identity;
using ResponseLibrary.Wrappers.Interfaces;

namespace Application.Features.Roles.Commands
{
    public class CreateRoleCommand : IRequest<IResponseWrapper>
    {
        public CreateRoleRequest CreateRole { get; set; }
    }

    public class CreateRoleCommandHandler(IRoleService roleService) : IRequestHandler<CreateRoleCommand, IResponseWrapper>
    {
        private readonly IRoleService _roleService = roleService;

        public async Task<IResponseWrapper> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
        {
            return await _roleService.CreateRoleAsync(request.CreateRole);
        }
    }
}
