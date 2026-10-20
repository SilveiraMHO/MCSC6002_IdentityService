using Application.Interfaces;
using MediatR;
using ResponseLibrary.Wrappers.Interfaces;

namespace Application.Features.Users.Queries
{
    public class GetAllUsersQuery : IRequest<IResponseWrapper>
    {
    }

    public class GetAllUsersQueryHandler(IUserService userService) : IRequestHandler<GetAllUsersQuery, IResponseWrapper>
    {
        private readonly IUserService _userService = userService;

        public async Task<IResponseWrapper> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            return await _userService.GetAllUsersAsync();
        }
    }
}
