using Application.Interfaces;
using Application.Pipelines.Interfaces;
using MediatR;
using ResponseLibrary.Models.Requests.Identity;
using ResponseLibrary.Wrappers.Interfaces;

namespace Application.Features.Users.Commands
{
    public class UpdateUserCommand : IRequest<IResponseWrapper>, IValidateMe
    {
        public UpdateUserRequest UpdateUser { get; set; }
    }

    public class UserUpdateCommandHandler(IUserService userService) : IRequestHandler<UpdateUserCommand, IResponseWrapper>
    {
        private readonly IUserService _userService = userService;

        public async Task<IResponseWrapper> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            return await _userService.UpdateUserAsync(request.UpdateUser);
        }
    }
}
