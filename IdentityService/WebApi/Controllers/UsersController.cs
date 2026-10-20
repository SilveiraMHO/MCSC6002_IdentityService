using Application.Features.Users.Commands;
using Application.Features.Users.Queries;
using AuthLibrary.Attributes;
using AuthLibrary.Constants.Authentication;
using Microsoft.AspNetCore.Mvc;
using ResponseLibrary.Models.Requests.Identity;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    public class UsersController : AppBaseController
    {
        [HttpPost("create")]
        [HasPermission(AppService.Identity, AppFeature.Users, AppAction.Create)]
        public async Task<IActionResult> CreateUserAsync([FromBody] CreateUserRequest request)
        {
            var response = await Sender.Send(new CreateUserCommand
            {
                CreateUser = request
            });

            if (response.IsSuccessful)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpGet("{userId}")]
        [HasPermission(AppService.Identity, AppFeature.Users, AppAction.Read)]
        public async Task<IActionResult> GetUserByIdAsync(string userId)
        {
            var response = await Sender.Send(new GetUserByIdQuery
            {
                UserId = userId
            });

            if (response.IsSuccessful)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpGet("get-all")]
        [HasPermission(AppService.Identity, AppFeature.Users, AppAction.Read)]
        public async Task<IActionResult> GetAllUserAsync()
        {
            var response = await Sender.Send(new GetAllUsersQuery());

            if (response.IsSuccessful)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpPut("update")]
        [HasPermission(AppService.Identity, AppFeature.Users, AppAction.Update)]
        public async Task<IActionResult> UpdateUserAsync([FromBody] UpdateUserRequest request)
        {
            var response = await Sender.Send(new UpdateUserCommand
            {
                UpdateUser = request
            });

            if (response.IsSuccessful)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpPut("change-password")]
        [HasPermission(AppService.Identity, AppFeature.Users, AppAction.Create)]
        public async Task<IActionResult> UpdateUserPasswordAsync([FromBody] ChangeUserPasswordRequest request)
        {
            var response = await Sender.Send(new UpdateUserPasswordCommand
            {
                ChangeUserPassword = request
            });

            if (response.IsSuccessful)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpPut("change-status")]
        [HasPermission(AppService.Identity, AppFeature.Users, AppAction.Update)]
        public async Task<IActionResult> UpdateUserStatusAsync([FromBody] ChangeUserStatusRequest request)
        {
            var response = await Sender.Send(new UpdateUserStatusCommand
            {
                ChangeUserStatus = request
            });

            if (response.IsSuccessful)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpPut("update-roles")]
        [HasPermission(AppService.Identity, AppFeature.Users, AppAction.Update)]
        public async Task<IActionResult> UpdateUserRolesAsync([FromBody] UpdateUserRolesRequest request)
        {
            var response = await Sender.Send(new UpdateUserRolesCommand
            {
                UpdateUserRoles = request
            });

            if (response.IsSuccessful)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
    }
}
