using Application.Interfaces;
using MediatR;
using ResponseLibrary.Models.Requests.Identity;
using ResponseLibrary.Wrappers.Interfaces;

namespace Application.Features.Token.Queries
{
    public class GetTokenQuery : IRequest<IResponseWrapper>
    {
        public TokenRequest Token { get; set; }
    }

    public class GetTokenQueryHandler(ITokenService tokenService) : IRequestHandler<GetTokenQuery, IResponseWrapper>
    {
        private readonly ITokenService _tokenService = tokenService;

        public async Task<IResponseWrapper> Handle(GetTokenQuery request, CancellationToken cancellationToken)
        {
            return await _tokenService.GetTokenAsync(request.Token);
        }
    }
}
