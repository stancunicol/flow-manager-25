using FlowManager.Application.Interfaces;
using FlowManager.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserRolesController : ControllerBase
    {
        private readonly IUserRoleService _service;

        public UserRolesController(IUserRoleService service)
        {
            _service = service;
        }

        [HttpGet("roles")]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _service.GetAllRoles();
            return Ok(roles);
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok("UserRoles controller is working");
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetRolesByUser(Guid userId)
        {
            Console.WriteLine($"[DEBUG] GetRolesByUser called with userId: {userId}");
            var roles = await _service.GetRolesByUser(userId);
            Console.WriteLine($"[DEBUG] Found {roles.Count} roles for user {userId}");
            return Ok(roles);
        }

        [HttpGet("role/{roleName}/users")]
        public async Task<IActionResult> GetUsersByRole(string roleName)
        {
            var users = await _service.GetUsersByRole(roleName);
            return Ok(users);
        }

        [HttpPost("user/{userId}/role/{roleName}")]
        public async Task<IActionResult> AddUserToRole(Guid userId, string roleName)
        {
            var success = await _service.AddUserToRole(userId, roleName);
            return success ? Ok() : BadRequest("Failed to add user to role");
        }

        [HttpDelete("user/{userId}/role/{roleName}")]
        public async Task<IActionResult> RemoveUserFromRole(Guid userId, string roleName)
        {
            var success = await _service.RemoveUserFromRole(userId, roleName);
            return success ? Ok() : BadRequest("Failed to remove user from role");
        }

        [HttpGet("user/{userId}/role/{roleName}/check")]
        public async Task<IActionResult> IsUserInRole(Guid userId, string roleName)
        {
            var isInRole = await _service.IsUserInRole(userId, roleName);
            return Ok(new { isInRole });
        }
    }
}
