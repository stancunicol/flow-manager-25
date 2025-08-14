using Microsoft.AspNetCore.Mvc;
using FlowManager.Application.Interfaces;
using FlowManager.Domain.Entities;
using FlowManager.Application.DTOs.Responses.Step;

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
        public async Task<ActionResult<IEnumerable<StepResponseDto>>> GetSteps()
        {
            var result = await _stepService.GetSteps();
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
        public async Task<ActionResult<StepResponseDto>> GetStep(Guid id)
        {
            var result = await _stepService.GetStep(id);
            return Ok(new
            {
                Result = result,
                Success = true,
                Message = "Step retrieved successfully.",
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpGet("flow/{flowId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<StepResponseDto>>> GetStepsByFlow(Guid flowId)
        {
            var result = await _stepService.GetStepsByFlow(flowId);
            return Ok(new
            {
                Result = result,
                Success = true,
                Message = "Steps retrieved successfully.",
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<StepResponseDto>> PostStep(Step step)
        {
            var result = await _stepService.PostStep(step);
            return Ok(new
            {
                Result = result,
                Success = true,
                Message = "Step posted succesfully.",
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PutStep(Guid id, Step step)
        {
            var result = await _stepService.PutStep(id, step);
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
        public async Task<IActionResult> DeleteStep(Guid id)
        {
            var result = await _stepService.DeleteStep(id);
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
            var result = await _stepService.AssignUserToStep(id, userId);
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
            var result = await _stepService.UnassignUserFromStep(id, userId);
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
            var result = await _stepService.AddStepToFlow(stepId, flowId);
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
            var result = await _stepService.RemoveStepFromFlow(stepId, flowId);
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
