using Application.Interfaces;
using Application.Pipelines.Interfaces;
using MediatR;
using ResponseLibrary.Models.Requests.Identity;
using ResponseLibrary.Wrappers.Interfaces;

namespace Application.Features.Users.Commands
{
    public class CreateUserCommand : IRequest<IResponseWrapper>, IValidateMe
    {
        public CreateUserRequest CreateUser { get; set; }
    }

    public class UserRegistrationCommandHandler(IUserService userService) : IRequestHandler<CreateUserCommand, IResponseWrapper>
    {
        private readonly IUserService _userService = userService;

        public async Task<IResponseWrapper> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            return await _userService.CreateUserAsync(request.CreateUser);
        }
    }
}
