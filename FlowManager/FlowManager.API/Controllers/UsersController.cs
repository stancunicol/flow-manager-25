using FlowManager.Application.DTOs;
using FlowManager.Application.Interfaces;
using FlowManager.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FlowManager.Application.DTOs;

namespace FlowManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _service;
        private readonly UserManager<User> _userManager;

        public UsersController(IUserService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _service.GetAllUsers(); // 👈 folosești serviciul tău

            var result = users.Select(u => new UserDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                //UserName = u.UserName,
                UserRoles = u.UserRoles.Select(ur => new UserRoleDto
                {
                    Role = new RoleDto
                    {
                        Name = ur.Role?.Name ?? ""
                    }
                }).ToList()
            });

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(Guid id)
        {
            var user = await _service.GetUserById(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpGet("email/{email}")]
        public async Task<IActionResult> GetUserByEmail(string email)
        {
            var user = await _service.GetUserByEmail(email);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpPost]
        public async Task<IActionResult> PostUser([FromBody] User user)
        {
            var created = await _service.CreateUser(user);
            if (created == null)
                return BadRequest("User with this email already exists");

            return CreatedAtAction(nameof(GetUser), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(Guid id, [FromBody] User user)
        {
            var updated = await _service.UpdateUser(id, user);
            return updated ? NoContent() : NotFound();
        }

        //[HttpPut("{id}/name/{name}/username/{username}/email/{email}")]
        //public async Task<IActionResult> UpdateUserProfile(Guid id, string name, string username, string email)
        //{
        //    var updated = await _service.UpdateUserProfile(id, name, username, email);
        //    return updated
        //        ? NoContent()
        //        : BadRequest("Email is already used by another user");
        //}

        [HttpPut("{id}/name/{name}/email/{email}")]
        public async Task<IActionResult> UpdateUserProfile(Guid id, string name,string email)
        {
            var updated = await _service.UpdateUserProfile(id, name, email);
            return updated
                ? NoContent()
                : BadRequest("Email is already used by another user");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var deleted = await _service.DeleteUser(id);
            return deleted ? NoContent() : NotFound();
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = await _service.GetUserById(Guid.Parse(userId));
            if (user == null)
                return NotFound();

            var dto = new UserProfileDto
            {
                Name = user.Name,
                Email = user.Email,
                UserName = user.UserName
            };

            return Ok(dto);
        }


    }
}
