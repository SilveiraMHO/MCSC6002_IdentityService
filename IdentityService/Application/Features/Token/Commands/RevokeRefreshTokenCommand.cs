using Application.Interfaces;
using MediatR;
using ResponseLibrary.Wrappers.Interfaces;

namespace Application.Features.Token.Commands
{
    public class RevokeRefreshTokenCommand : IRequest<IResponseWrapper>
    {
        public string UserId { get; set; }
    }

    public class RevokeRefreshTokenCommandHandler(ITokenService tokenService) : IRequestHandler<RevokeRefreshTokenCommand, IResponseWrapper>
    {
        private readonly ITokenService _tokenService = tokenService;

        public async Task<IResponseWrapper> Handle(RevokeRefreshTokenCommand request, CancellationToken cancellationToken)
        {
            return await _tokenService.RevokeRefreshTokenAsync(request.UserId);
        }
    }
}
