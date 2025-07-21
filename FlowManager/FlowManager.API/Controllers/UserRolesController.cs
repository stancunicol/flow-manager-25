using FlowManager.Application.Interfaces;
using FlowManager.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace FlowManager.Controllers
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

        [HttpGet]
        public async Task<IActionResult> GetUserRoles()
        {
            var roles = await _service.GetAllUserRoles();
            return Ok(roles);
        }

        [HttpGet("user/{userId}/role/{roleId}")]
        public async Task<IActionResult> GetUserRole(Guid userId, Guid roleId)
        {
            var role = await _service.GetUserRole(userId, roleId);
            if (role == null) return NotFound();
            return Ok(role);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetRolesByUser(Guid userId)
        {
            var roles = await _service.GetRolesByUser(userId);
            return Ok(roles);
        }

        [HttpGet("role-name/{roleName}")]
        public async Task<IActionResult> GetUsersByRole(string roleName)
        {
            var users = await _service.GetUsersByRole(roleName);
            return Ok(users);
        }

        [HttpPut("user/{userId}/role/{roleId}")]
        public async Task<IActionResult> UpdateUserRole(Guid userId, Guid roleId, [FromBody] UserRole userRole)
        {
            var updated = await _service.UpdateUserRole(userId, roleId, userRole);
            return updated ? NoContent() : NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> PostUserRole([FromBody] UserRole userRole)
        {
            var created = await _service.CreateUserRole(userRole);
            if (created == null)
            {
                return BadRequest("User already has this role");
            }

            return CreatedAtAction(nameof(GetUserRole), new { userId = created.UserId, roleId = created.RoleId }, created);
        }

        [HttpDelete("user/{userId}/role/{roleId}")]
        public async Task<IActionResult> DeleteUserRole(Guid userId, Guid roleId)
        {
            var deleted = await _service.DeleteUserRole(userId, roleId);
            return deleted ? NoContent() : NotFound();
        }
    }
}
