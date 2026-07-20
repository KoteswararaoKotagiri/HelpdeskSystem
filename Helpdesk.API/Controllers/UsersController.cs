using Helpdesk.Application.Common;
using Helpdesk.Application.Features.Users.DTOs;
using Helpdesk.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Helpdesk.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ICurrentUserService _currentUser;

        public UsersController(
            IUserService userService,
            ICurrentUserService currentUser)
        {
            _userService = userService;
            _currentUser = currentUser;
        }

        // ---------- Admin-only user administration ----------

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUsers([FromQuery] UserListQueryDto query)
        {
            return ToResponse(await _userService.GetUsersAsync(query));
        }

        [HttpGet("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUser(Guid id)
        {
            return ToResponse(await _userService.GetUserByIdAsync(id));
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUser(Guid id, UpdateUserRequestDto request)
        {
            return ToResponse(await _userService.UpdateUserAsync(id, request));
        }

        // Soft delete = deactivate. Records are never removed; reactivate via the edit endpoint.
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeactivateUser(Guid id)
        {
            return ToResponse(await _userService.SetActiveStatusAsync(id, false));
        }

        [HttpPut("{id:guid}/role")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChangeRole(Guid id, ChangeRoleRequestDto request)
        {
            return ToResponse(await _userService.ChangeRoleAsync(id, request.RoleId));
        }

        [HttpPut("{id:guid}/department")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChangeDepartment(Guid id, ChangeDepartmentRequestDto request)
        {
            return ToResponse(await _userService.ChangeDepartmentAsync(id, request.DepartmentId));
        }

        // ---------- Self-service (any authenticated user) ----------

        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequestDto request)
        {
            var result = await _userService.ChangePasswordAsync(_currentUser.UserId, request);

            return result.IsSuccess
                ? Ok(new { message = "Password changed successfully." })
                : ToResponse(result);
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            return ToResponse(await _userService.GetProfileAsync(_currentUser.UserId));
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile(UpdateProfileRequestDto request)
        {
            return ToResponse(await _userService.UpdateProfileAsync(_currentUser.UserId, request));
        }

        // ---------- Result -> HTTP mapping ----------

        private IActionResult ToResponse<T>(Result<T> result)
        {
            return result.IsSuccess ? Ok(result.Data) : ToResponse((Result)result);
        }

        private IActionResult ToResponse(Result result)
        {
            return result.Status switch
            {
                ResultStatus.Success => Ok(),
                ResultStatus.NotFound => NotFound(new { error = result.Error }),
                ResultStatus.Conflict => Conflict(new { error = result.Error }),
                _ => BadRequest(new { error = result.Error })
            };
        }
    }
}
