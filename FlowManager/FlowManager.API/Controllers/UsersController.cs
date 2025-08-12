using FlowManager.Application.DTOs;
using FlowManager.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController] 
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        
        [HttpGet("moderators")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllModerators()
        {
            try
            {
                var moderators = await _userService.GetAllModeratorsAsync();
                return Ok(moderators);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        
        [HttpGet("admins")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllAdmins()
        {
            try
            {
                var admins = await _userService.GetAllAdminsAsync();
                return Ok(admins);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        
        [HttpGet("filtered")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsersFiltered(
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? role = null)
        {
            try
            {
                var users = await _userService.GetAllUsersFilteredAsync(searchTerm, role);
                return Ok(users);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUserById(Guid id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound($"User with ID {id} not found");
                }
                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        
        [HttpPost]
        public async Task<ActionResult<UserDto>> AddUser([FromBody] CreateUserDto createUserDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var user = await _userService.AddUserAsync(createUserDto);
                if (user == null)
                {
                    return BadRequest("Failed to create user");
                }

                return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        
        [HttpPatch("{id}")]
        public async Task<ActionResult> UpdateUser(Guid id, [FromBody] UpdateUserDto updateUserDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _userService.UpdateUserAsync(id, updateUserDto);
                if (!result)
                {
                    return NotFound($"User with ID {id} not found");
                }

                return NoContent(); 
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUser(Guid id)
        {
            try
            {
                var result = await _userService.DeleteUserAsync(id);
                if (!result)
                {
                    return NotFound($"User with ID {id} not found");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpPatch("{id}/restore")]
        public async Task<ActionResult> RestoreUser(Guid id)
        {
            try
            {
                var result = await _userService.RestoreUserAsync(id);
                if (!result)
                {
                    return NotFound($"Deleted user with ID {id} not found");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}