using Application.Features.Roles.Commands;
using Application.Features.Roles.Queries;
using AuthLibrary.Attributes;
using AuthLibrary.Constants.Authentication;
using Microsoft.AspNetCore.Mvc;
using ResponseLibrary.Models.Requests.Identity;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    public class RolesController : AppBaseController
    {
        [HttpPost("create")]
        [HasPermission(AppService.Identity, AppFeature.Roles, AppAction.Create)]
        public async Task<IActionResult> CreateRoleAsync(CreateRoleRequest request)
        {
            var response = await Sender.Send(new CreateRoleCommand
            {
                CreateRole = request
            });

            if (response.IsSuccessful)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpDelete("delete")]
        [HasPermission(AppService.Identity, AppFeature.Roles, AppAction.Delete)]
        public async Task<IActionResult> DeleteRoleAsync(string roleId)
        {
            var response = await Sender.Send(new DeleteRoleCommand
            {
                RoleId = roleId
            });

            if (response.IsSuccessful)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpPut("update")]
        [HasPermission(AppService.Identity, AppFeature.Roles, AppAction.Update)]
        public async Task<IActionResult> UpdateRoleAsync(UpdateRoleRequest request)
        {
            var response = await Sender.Send(new UpdateRoleCommand
            {
                UpdateRole = request
            });

            if (response.IsSuccessful)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpGet("get-all")]
        [HasPermission(AppService.Identity, AppFeature.Roles, AppAction.Read)]
        public async Task<IActionResult> GetAllRolesAsync()
        {
            var response = await Sender.Send(new GetAllRolesQuery());

            if (response.IsSuccessful)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpGet("{roleId}")]
        [HasPermission(AppService.Identity, AppFeature.Roles, AppAction.Read)]
        public async Task<IActionResult> GetRolesByIdAsync(string roleId)
        {
            var response = await Sender.Send(new GetRoleByIdQuery
            {
                RoleId = roleId
            });

            if (response.IsSuccessful)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpGet("get-permissions/{roleId}")]
        [HasPermission(AppService.Identity, AppFeature.Roles, AppAction.Read)]
        public async Task<IActionResult> GePermissionsAsync(string roleId)
        {
            var response = await Sender.Send(new GetPermissionsQuery
            {
                RoleId = roleId
            });

            if (response.IsSuccessful)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpPut("update-permissions")]
        [HasPermission(AppService.Identity, AppFeature.Roles, AppAction.Update)]
        public async Task<IActionResult> UpdateRolePermissionsAsync(UpdateRoleClaimsRequest request)
        {
            var response = await Sender.Send(new UpdateRolePermissionsCommand
            {
                UpdateRoleClaims = request
            });

            if (response.IsSuccessful)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
    }
}
