using Application.Interfaces;
using MediatR;
using ResponseLibrary.Models.Requests.Identity;
using ResponseLibrary.Wrappers.Interfaces;

namespace Application.Features.Token.Queries
{
    public class GetRefreshTokenQuery : IRequest<IResponseWrapper>
    {
        public RefreshTokenRequest RefreshToken { get; set; }
    }

    public class GetRefreshTokenQueryHandler(ITokenService tokenService) : IRequestHandler<GetRefreshTokenQuery, IResponseWrapper>
    {
        private readonly ITokenService _tokenService = tokenService;

        public async Task<IResponseWrapper> Handle(GetRefreshTokenQuery request, CancellationToken cancellationToken)
        {
            return await _tokenService.GetRefreshTokenAsync(request.RefreshToken);
        }
    }
}
