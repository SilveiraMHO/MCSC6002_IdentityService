using Application.Interfaces;
using MediatR;
using ResponseLibrary.Models.Requests.Identity;
using ResponseLibrary.Wrappers.Interfaces;

namespace Application.Features.Users.Commands
{
    public class UpdateUserRolesCommand : IRequest<IResponseWrapper>
    {
        public UpdateUserRolesRequest UpdateUserRoles { get; set; }
    }

    public class UpdateUserRolesCommandHandler(IUserService userService) : IRequestHandler<UpdateUserRolesCommand, IResponseWrapper>
    {
        private readonly IUserService _userService = userService;

        public async Task<IResponseWrapper> Handle(UpdateUserRolesCommand request, CancellationToken cancellationToken)
        {
            return await _userService.UpdateUserRolesAsync(request.UpdateUserRoles);
        }
    }
}
