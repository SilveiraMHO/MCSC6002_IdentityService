using Application.Features.Token.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResponseLibrary.Models.Requests.Identity;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    public class TokenController : AppBaseController
    {
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> LoginAsync([FromBody] TokenRequest request)
        {
            var response = await Sender.Send(new GetTokenQuery
            {
                Token = request
            });

            if (response.IsSuccessful)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> GetRefreshTokenAsync([FromBody] RefreshTokenRequest request)
        {
            var response = await Sender.Send(new GetRefreshTokenQuery
            {
                RefreshToken = request
            });

            if (response.IsSuccessful)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
    }
}
