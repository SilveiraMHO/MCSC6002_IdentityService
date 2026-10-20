using Application.Interfaces;
using MediatR;
using ResponseLibrary.Models.Requests.Identity;
using ResponseLibrary.Wrappers.Interfaces;

namespace Application.Features.Users.Commands
{
    public class UpdateUserStatusCommand : IRequest<IResponseWrapper>
    {
        public ChangeUserStatusRequest ChangeUserStatus { get; set; }
    }

    public class UpdateUserStatusCommandHandler(IUserService userService) : IRequestHandler<UpdateUserStatusCommand, IResponseWrapper>
    {
        private readonly IUserService _userService = userService;

        public async Task<IResponseWrapper> Handle(UpdateUserStatusCommand request, CancellationToken cancellationToken)
        {
            return await _userService.ChangeUserStatusAsync(request.ChangeUserStatus);
        }
    }
}
