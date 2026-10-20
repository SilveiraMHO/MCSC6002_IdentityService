using ResponseLibrary.Models.Requests.Identity;
using ResponseLibrary.Wrappers.Interfaces;

namespace Application.Interfaces
{
    public interface ITokenService
    {
        Task<IResponseWrapper> GetTokenAsync(TokenRequest tokenRequest);
        Task<IResponseWrapper> GetRefreshTokenAsync(RefreshTokenRequest refreshTokenRequest);
    }
}
