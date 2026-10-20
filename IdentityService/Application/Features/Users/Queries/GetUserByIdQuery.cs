using Application.Interfaces;
using MediatR;
using ResponseLibrary.Wrappers.Interfaces;

namespace Application.Features.Users.Queries
{
    public class GetUserByIdQuery : IRequest<IResponseWrapper>
    {
        public string UserId { get; set; }
    }

    public class GetUserByIdQueryHandler(IUserService userService) : IRequestHandler<GetUserByIdQuery, IResponseWrapper>
    {
        private readonly IUserService _userService = userService;

        public async Task<IResponseWrapper> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            return await _userService.GetUserByIdAsync(request.UserId);
        }
    }
}
