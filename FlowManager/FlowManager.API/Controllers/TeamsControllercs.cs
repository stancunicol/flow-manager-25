using FlowManager.Application.Interfaces;
using FlowManager.Application.IServices;
using FlowManager.Shared.DTOs.Requests.Team;
using FlowManager.Shared.DTOs.Responses.Team;
using Microsoft.AspNetCore.Mvc;

namespace FlowManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeamsController : ControllerBase
    {
        private readonly ITeamService _teamService;

        public TeamsController(ITeamService teamService)
        {
            _teamService = teamService;
        }

        // ==========================================
        // CRUD OPERATIONS
        // ==========================================

        /// <summary>
        /// Get all teams with optional filtering, sorting and pagination
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTeamsQueriedAsync([FromQuery] QueriedTeamRequestDto payload)
        {
            var result = await _teamService.GetAllTeamsQueriedAsync(payload);

            if (result.Data == null || !result.Data.Any())
            {
                return NotFound(new
                {
                    Result = new List<TeamResponseDto>(),
                    Success = false,
                    Message = "No teams found matching the criteria.",
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                Result = result,
                Success = true,
                Message = "Teams retrieved successfully.",
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Get all teams (simple list without pagination)
        /// </summary>
        [HttpGet("all")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllTeamsAsync()
        {
            var result = await _teamService.GetAllTeamsAsync();

            if (result == null || !result.Any())
            {
                return NotFound(new
                {
                    Result = new List<TeamResponseDto>(),
                    Success = false,
                    Message = "No teams found.",
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                Result = result,
                Success = true,
                Message = "Teams retrieved successfully.",
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Get team by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTeamAsync(Guid id)
        {
            var result = await _teamService.GetTeamByIdAsync(id);
            return Ok(new
            {
                Result = result,
                Success = true,
                Message = "Team retrieved successfully.",
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Get team by ID with all users details
        /// </summary>
        [HttpGet("{id}/details")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTeamWithUsersAsync(Guid id)
        {
            var result = await _teamService.GetTeamWithUsersAsync(id);
            return Ok(new
            {
                Result = result,
                Success = true,
                Message = "Team with users retrieved successfully.",
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Get team by name
        /// </summary>
        [HttpGet("by-name/{name}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTeamByNameAsync(string name)
        {
            var result = await _teamService.GetTeamByNameAsync(name);
            return Ok(new
            {
                Result = result,
                Success = true,
                Message = "Team retrieved successfully.",
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Create a new team
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PostTeamAsync([FromBody] PostTeamRequestDto payload)
        {
            var result = await _teamService.AddTeamAsync(payload);
            return CreatedAtAction(nameof(GetTeamAsync), new { id = result.Id }, new
            {
                Result = result,
                Success = true,
                Message = "Team created successfully.",
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Update team (partial update) - supports user management
        /// </summary>
        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PatchTeamAsync(Guid id, [FromBody] PatchTeamRequestDto payload)
        {
            var result = await _teamService.UpdateTeamAsync(id, payload);
            return Ok(new
            {
                Result = result,
                Success = true,
                Message = "Team updated successfully.",
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Delete team (soft delete) - automatically removes users from team
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteTeamAsync(Guid id)
        {
            var result = await _teamService.DeleteTeamAsync(id);
            return Ok(new
            {
                Result = result,
                Success = true,
                Message = "Team deleted successfully. All users have been removed from the team.",
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Restore team (undo soft delete)
        /// </summary>
        [HttpPatch("{id}/restore")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RestoreTeamAsync(Guid id)
        {
            var result = await _teamService.RestoreTeamAsync(id);
            return Ok(new
            {
                Result = result,
                Success = true,
                Message = "Team restored successfully.",
                Timestamp = DateTime.UtcNow
            });
        }

        // ==========================================
        // USER MANAGEMENT ENDPOINTS
        // ==========================================
        // Nota: Aceste endpoint-uri sunt opționale deoarece managementul 
        // userilor se poate face prin PATCH cu UserIds, dar sunt utile 
        // pentru operațiuni granulare

        /// <summary>
        /// Add multiple users to team (alternative to PATCH)
        /// </summary>
        [HttpPost("{teamId}/users")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddUsersToTeamAsync(Guid teamId, [FromBody] List<Guid> userIds)
        {
            // Folosește PATCH cu logica existentă
            var patchPayload = new PatchTeamRequestDto { UserIds = userIds };
            var result = await _teamService.UpdateTeamAsync(teamId, patchPayload);

            return Ok(new
            {
                Result = result,
                Success = true,
                Message = $"{userIds.Count} user(s) added to team successfully.",
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Remove all users from team
        /// </summary>
        [HttpDelete("{teamId}/users")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RemoveAllUsersFromTeamAsync(Guid teamId)
        {
            // Folosește PATCH cu listă goală pentru a elimina toți userii
            var patchPayload = new PatchTeamRequestDto { UserIds = new List<Guid>() };
            var result = await _teamService.UpdateTeamAsync(teamId, patchPayload);

            return Ok(new
            {
                Result = result,
                Success = true,
                Message = "All users removed from team successfully.",
                Timestamp = DateTime.UtcNow
            });
        }

        // ==========================================
        // STATISTICS ENDPOINTS (opționale)
        // ==========================================

        /// <summary>
        /// Get team statistics
        /// </summary>
        [HttpGet("{id}/stats")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTeamStatsAsync(Guid id)
        {
            var team = await _teamService.GetTeamWithUsersAsync(id);

            var stats = new
            {
                TeamId = team.Id,
                TeamName = team.Name,
                TotalUsers = team.UsersCount,
                ActiveUsers = team.Users?.Count(u => u.DeletedAt == null) ?? 0,
                InactiveUsers = team.Users?.Count(u => u.DeletedAt != null) ?? 0,
                CreatedAt = team.CreatedAt,
                LastUpdated = team.UpdatedAt
            };

            return Ok(new
            {
                Result = stats,
                Success = true,
                Message = "Team statistics retrieved successfully.",
                Timestamp = DateTime.UtcNow
            });
        }
    }
}