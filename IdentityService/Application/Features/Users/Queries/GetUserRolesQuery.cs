using Application.Interfaces;
using MediatR;
using ResponseLibrary.Wrappers.Interfaces;

namespace Application.Features.Users.Queries
{
    public class GetUserRolesQuery : IRequest<IResponseWrapper>
    {
        public string UserId { get; set; }
    }

    public class GetUserRolesQueryHandler(IUserService userService) : IRequestHandler<GetUserRolesQuery, IResponseWrapper>
    {
        private readonly IUserService _userService = userService;

        public async Task<IResponseWrapper> Handle(GetUserRolesQuery request, CancellationToken cancellationToken)
        {
            return await _userService.GetUserRolesAsync(request.UserId);
        }
    }
}
