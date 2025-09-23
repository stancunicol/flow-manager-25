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

        [HttpGet("queried")]
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

        [HttpGet("moderators/queried/{stepId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetModeratorTeamsQueriedAsync(Guid stepId, [FromQuery] QueriedTeamRequestDto payload)
        {
            var result = await _teamService.GetAllModeratorTeamsQueriedAsync(stepId, payload);

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

        [HttpGet("queried/splitUsers/{teamId}/byStep/{stepId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSplitUsersByTeamIdQueriedAsync(Guid stepId, Guid teamId, [FromQuery] QueriedTeamRequestDto payload)
        {
            var result = await _teamService.GetSplitUsersByTeamIdAsync(stepId, teamId, payload);

            return Ok(new
            {
                Result = result,
                Success = true,
                Message = "Users succesfully retrieved successfully.",
                Timestamp = DateTime.UtcNow
            });
        }

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

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PostTeamAsync([FromBody] PostTeamRequestDto payload)
        {
            var result = await _teamService.AddTeamAsync(payload);
            return Created($"api/teams/{result.Id}", new
            {
                Result = result,
                Success = true,
                Message = "Team created successfully.",
                Timestamp = DateTime.UtcNow
            });
        }

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

        [HttpPost("{teamId}/users")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddUsersToTeamAsync(Guid teamId, [FromBody] List<Guid> userIds)
        {
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

        [HttpDelete("{teamId}/users")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RemoveAllUsersFromTeamAsync(Guid teamId)
        {
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