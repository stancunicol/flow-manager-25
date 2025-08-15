using Microsoft.AspNetCore.Mvc;
using FlowManager.Application.Interfaces;
using FlowManager.Domain.Entities;
using FlowManager.Application.DTOs.Responses.Step;
using FlowManager.Application.DTOs.Requests.Step;

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

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetStepsQueriedAsync([FromQuery] QueriedStepRequestDto payload)
        {
            var result = await _stepService.GetAllStepsQueriedAsync(payload);

            if(result.Data == null || !result.Data.Any())
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PostStepAsync([FromBody] PostStepRequestDto payload)
        {
            var result = await _stepService.PostStepAsync(payload);
            return Ok(new
            {
                Result = result,
                Success = true,
                Message = "Step posted succesfully.",
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PatchStepAsync(Guid id,[FromBody] PatchStepRequestDto payload)
        {
            var result = await _stepService.PatchStepAsync(id, payload);
            return Ok(new
            {
                Result = result,
                Success = true,
                Message = "Step put succesfully.",
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
                Message = "Step deleted succesfully.",
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpPost("{id}/assign-user/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AssignUserToStep(Guid id, Guid userId)
        {
            var result = await _stepService.AssignUserToStepAsync(id, userId);
            return Ok(new
            {
                Result = result,
                Success = true,
                Message = "User assigned to step succesfully.",
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpDelete("{id}/unassign-user/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UnassignUserFromStep(Guid id, Guid userId)
        {
            var result = await _stepService.UnassignUserFromStepAsync(id, userId);
            return Ok(new
            {
                Result = result,
                Success = true,
                Message = "User unassigned from step succesfully.",
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpPost("{stepId}/add-to-flow/{flowId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddStepToFlow(Guid stepId, Guid flowId, [FromQuery] int order = 0)
        {
            var result = await _stepService.AddStepToFlowAsync(stepId, flowId);
            return Ok(new
            {
                Result = result,
                Success = true,
                Message = "Step added to flow succesfully.",
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpDelete("{stepId}/remove-from-flow/{flowId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RemoveStepFromFlow(Guid stepId, Guid flowId)
        {
            var result = await _stepService.RemoveStepFromFlowAsync(stepId, flowId);
            return Ok(new
            {
                Result = result,
                Success = true,
                Message = "Step removed from flow succesfully.",
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpPatch("{stepId}/restore-from-flow/{flowId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RestoreStepFromFlow(Guid stepId, Guid flowId)
        {
            var result = await _stepService.RestoreStepToFlowAsync(stepId, flowId);
            return Ok(new
            {
                Result = result,
                Success = true,
                Message = "Step removed from flow succesfully.",
                Timestamp = DateTime.UtcNow
            });
        }
    }
}
