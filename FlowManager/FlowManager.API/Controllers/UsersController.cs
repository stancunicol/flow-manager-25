using FlowManager.Application.DTOs;
using FlowManager.Application.DTOs.Requests.User;
using FlowManager.Application.DTOs.Responses.User;
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
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetAllUsers()
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
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetAllModerators()
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
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetAllAdmins()
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
        public async Task<IActionResult> GetAllUsersFiltered(
            [FromQuery] QueriedUserRequestDto payload)
        {
            try
            {
                var users = await _userService.GetAllUsersFilteredAsync(payload);
                return Ok(users);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(Guid id)
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
        public async Task<IActionResult> AddUser([FromBody] PostUserRequestDto payload)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var user = await _userService.AddUserAsync(payload);
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
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] PatchUserRequestDto payload)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _userService.UpdateUserAsync(id, payload);
                if (result == null)
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
                if (result == null)
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
                if (result == null)
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