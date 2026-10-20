using Application.Interfaces;
using MediatR;
using ResponseLibrary.Models.Requests.Identity;
using ResponseLibrary.Wrappers.Interfaces;

namespace Application.Features.Users.Commands
{
    public class UpdateUserPasswordCommand : IRequest<IResponseWrapper>
    {
        public ChangeUserPasswordRequest ChangeUserPassword { get; set; }
    }

    public class UpdateUserPasswordCommandHandler(IUserService userService) : IRequestHandler<UpdateUserPasswordCommand, IResponseWrapper>
    {
        private readonly IUserService _userService = userService;

        public async Task<IResponseWrapper> Handle(UpdateUserPasswordCommand request, CancellationToken cancellationToken)
        {
            return await _userService.ChangeUserPasswordAsync(request.ChangeUserPassword);
        }
    }
}
