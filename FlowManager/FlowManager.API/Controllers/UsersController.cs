using FlowManager.Application.Interfaces;
using FlowManager.Shared.DTOs.Requests.User;
using FlowManager.Shared.DTOs.Responses.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowManager.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllUsersAsync()
        {
            var result = await _userService.GetAllUsersAsync();

            if(result == null || !result.Any())
            {
                return NotFound(new
                {
                    Result = new List<UserResponseDto>(),
                    Success = false,
                    Message = "No users found.",
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                Result = result,
                Success = true,
                Message = "Users retrieved successfully.",
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpGet("moderators")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllModeratorsAsync()
        {
            var result = await _userService.GetAllModeratorsAsync();

            if(result == null || !result.Any())
            {
                return NotFound(new
                {
                    Result = new List<UserResponseDto>(),
                    Success = false,
                    Message = "No moderators found.",
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                Result = result,
                Success = true,
                Message = "Moderators retrieved successfully.",
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpGet("admins")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllAdminsAsync()
        {
            var result = await _userService.GetAllAdminsAsync();

            if(result == null || !result.Any())
            {
                return NotFound(new
                {
                    Result = new List<UserResponseDto>(),
                    Success = false,
                    Message = "No admins found.",
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                Result = result,
                Success = true,
                Message = "Admins retrieved successfully.",
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpGet("queried")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllUsersQueriedAsync([FromQuery] QueriedUserRequestDto payload)
        {
            var result = await _userService.GetAllUsersQueriedAsync(payload);

            if(result.Data == null || !result.Data.Any())
            {
                return NotFound(new
                {
                    Result = new List<UserResponseDto>(),
                    Success = false,
                    Message = "No users found matching the criteria.",
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                Result = result,
                Success = true,
                Message = "Users retrieved successfully.",
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserByIdAsync(Guid id)
        {
            var result = await _userService.GetUserByIdAsync(id);

            return Ok(new
            {
                Result = result,
                Success = true,
                Message = "User retrieved successfully.",
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PostUserAsync([FromBody] PostUserRequestDto payload)
        {
            var result = await _userService.AddUserAsync(payload);

            return Created($"/api/users/{result.Id}", new
            {
                Result = result,
                Success = true,
                Message = "User created successfully.",
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PatchUserAsync(Guid id, [FromBody] PatchUserRequestDto payload)
        {
            var result = await _userService.UpdateUserAsync(id, payload);

            return Ok(new
            {
                Result = result,
                Success = true,
                Message = "User updated successfully.",
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteUserAsync(Guid id)
        {
            var result = await _userService.DeleteUserAsync(id);

            return Ok(new
            {
                Result = result,
                Success = true,
                Message = "User deleted successfully.",
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpPatch("{id}/restore")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RestoreUserAsync(Guid id)
        {
            var result = await _userService.RestoreUserAsync(id);

            return Ok(new
            {
                Result = result,
                Success = true,
                Message = "User restored successfully.",
                Timestamp = DateTime.UtcNow
            });
        }
    }
}