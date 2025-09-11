using Microsoft.AspNetCore.Mvc;
using FlowManager.Application.Interfaces;
using FlowManager.Domain.Entities;
using FlowManager.Shared.DTOs.Requests.Step;
using FlowManager.Shared.DTOs.Responses.Step;

namespace FlowManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StepsController : ControllerBase
    {
        private readonly IStepService _stepService;

        public StepsController(IStepService stepService)
        {
            _stepService = stepService;
        }

        [HttpGet("queried")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetStepsQueriedAsync([FromQuery] QueriedStepRequestDto payload)
        {
            var result = await _stepService.GetAllStepsQueriedAsync(payload);

            if (result.Data == null || !result.Data.Any())
            {
                return NotFound(new
                {
                    Result = new List<StepResponseDto>(),
                    Success = false,
                    Message = "No steps found matching the criteria.",
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                Result = result,
                Success = true,
                Message = "Steps retrieved successfully.",
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpGet("include-teams-users/queried")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllStepsIncludeUsersAndTeamsQueriedAsync([FromQuery] QueriedStepRequestDto payload)
        {
            var result = await _stepService.GetAllStepsIncludeUsersAndTeamsQueriedAsync(payload);

            if (result.Data == null || !result.Data.Any())
            {
                return NotFound(new
                {
                    Result = new List<StepResponseDto>(),
                    Success = false,
                    Message = "No steps found matching the criteria.",
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                Result = result,
                Success = true,
                Message = "Steps retrieved successfully.",
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpGet("all")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllStepsAsync()
        {
            var result = await _stepService.GetStepsAsync();

            if (result == null || !result.Any())
            {
                return NotFound(new
                {
                    Result = new List<StepResponseDto>(),
                    Success = false,
                    Message = "No steps found.",
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                Result = result,
                Success = true,
                Message = "Steps retrieved successfully.",
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetStepAsync(Guid id)
        {
            var result = await _stepService.GetStepAsync(id);
            return Ok(new
            {
                Result = result,
                Success = true,
                Message = "Step retrieved successfully.",
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PostStepAsync([FromBody] PostStepRequestDto payload)
        {
            var result = await _stepService.PostStepAsync(payload);
            return Ok(new
            {
                Result = result,
                Success = true,
                Message = "Step created successfully.",
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PatchStepAsync(Guid id, [FromBody] PatchStepRequestDto payload)
        {
            var result = await _stepService.PatchStepAsync(id, payload);
            return Ok(new
            {
                Result = result,
                Success = true,
                Message = "Step updated successfully.",
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteStepAsync(Guid id)
        {
            var result = await _stepService.DeleteStepAsync(id);
            return Ok(new
            {
                Result = result,
                Success = true,
                Message = "Step deleted successfully.",
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpPost("{stepId:guid}/assign-user/{userId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AssignUserToStep(Guid stepId, Guid userId)
        {
            var result = await _stepService.AssignUserToStepAsync(stepId, userId);
            return Ok(new
            {
                Result = result,
                Success = true,
                Message = "User assigned to step successfully.",
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpDelete("{stepId:guid}/unassign-user/{userId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UnassignUserFromStep(Guid stepId, Guid userId)
        {
            var result = await _stepService.UnassignUserFromStepAsync(stepId, userId);
            return Ok(new
            {
                Result = result,
                Success = true,
                Message = "User unassigned to step successfully.",
                Timestamp = DateTime.UtcNow
            });
        }
    }
}