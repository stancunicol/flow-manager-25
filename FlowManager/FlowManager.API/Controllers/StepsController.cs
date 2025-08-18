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

        // ==========================================
        // CRUD OPERATIONS
        // ==========================================

        /// <summary>
        /// Get all steps with optional filtering, sorting and pagination
        /// </summary>
        [HttpGet]
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

        /// <summary>
        /// Get all steps (simple list without pagination)
        /// </summary>
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

        /// <summary>
        /// Get step by ID
        /// </summary>
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

        /// <summary>
        /// Create a new step with users, teams and flows
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PostStepAsync([FromBody] PostStepRequestDto payload)
        {
            var result = await _stepService.PostStepAsync(payload);
            return CreatedAtAction(nameof(GetStepAsync), new { id = result.Id }, new
            {
                Result = result,
                Success = true,
                Message = "Step created successfully.",
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Update step (name, users, teams) - Complete management through PATCH
        /// </summary>
        /// <remarks>
        /// Examples:
        /// 
        /// Update only name:
        /// {
        ///   "name": "New Step Name"
        /// }
        /// 
        /// Update users (replace all):
        /// {
        ///   "userIds": ["user1-id", "user2-id"]
        /// }
        /// 
        /// Remove all users:
        /// {
        ///   "userIds": []
        /// }
        /// 
        /// Update teams (replace all):
        /// {
        ///   "teamIds": ["team1-id", "team2-id"]
        /// }
        /// 
        /// Remove all teams:
        /// {
        ///   "teamIds": []
        /// }
        /// 
        /// Update everything:
        /// {
        ///   "name": "Updated Step",
        ///   "userIds": ["user1-id"],
        ///   "teamIds": ["team1-id"]
        /// }
        /// </remarks>
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

        /// <summary>
        /// Delete step (soft delete)
        /// </summary>
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

        // ==========================================
        // FLOW MANAGEMENT (păstrat pentru că nu e redundant)
        // ==========================================

        /// <summary>
        /// Add step to flow
        /// </summary>
        [HttpPost("{stepId}/flows/{flowId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddStepToFlowAsync(Guid stepId, Guid flowId)
        {
            var result = await _stepService.AddStepToFlowAsync(stepId, flowId);
            return Ok(new
            {
                Result = result,
                Success = true,
                Message = "Step added to flow successfully.",
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Remove step from flow
        /// </summary>
        [HttpDelete("{stepId}/flows/{flowId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RemoveStepFromFlowAsync(Guid stepId, Guid flowId)
        {
            var result = await _stepService.RemoveStepFromFlowAsync(stepId, flowId);
            return Ok(new
            {
                Result = result,
                Success = true,
                Message = "Step removed from flow successfully.",
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Restore step to flow (undo soft delete)
        /// </summary>
        [HttpPatch("{stepId}/flows/{flowId}/restore")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RestoreStepToFlowAsync(Guid stepId, Guid flowId)
        {
            var result = await _stepService.RestoreStepToFlowAsync(stepId, flowId);
            return Ok(new
            {
                Result = result,
                Success = true,
                Message = "Step restored to flow successfully.",
                Timestamp = DateTime.UtcNow
            });
        }
    }
}