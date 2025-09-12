using Microsoft.AspNetCore.Mvc;
using FlowManager.Application.Interfaces;
using FlowManager.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using FlowManager.Shared.DTOs.Requests.Flow;
using FlowManager.Shared.DTOs.Responses.Flow;
using FlowManager.Shared.DTOs.Responses.Step;

namespace FlowManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlowsController : ControllerBase
    {
        private readonly IFlowService _flowService;

        public FlowsController(IFlowService flowService)
        {
            _flowService = flowService;
        }

        [HttpGet("queried")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFlowsQueriedAsync([FromQuery] QueriedFlowRequestDto payload)
        {
            var result = await _flowService.GetAllFlowsQueriedAsync(payload);

            if(result.Data == null || !result.Data.Any())
            {
                return NotFound(new
                {
                    Result = new List<FlowResponseDto>(),
                    Message = "No flows found matching the criteria.",
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                Result = result,
                Success = true,
                Message = "Flows retreived succesfully.",
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFlowAsync(Guid id)
        {
            var result = await _flowService.GetFlowByIdAsync(id);

            return Ok(new
            {
                Result = result,
                Success = true,
                Message = "Flow retreived succesfully.",
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpGet("includeSteps/{flowId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFlowByIdIncludeStepsAsync(Guid flowId)
        {
            var result = await _flowService.GetFlowByIdIncludeStepsAsync(flowId);

            return Ok(new
            {
                Result = result,
                Success = true,
                Message = "Flow retreived succesfully.",
                Timestamp = DateTime.UtcNow
            });
        }

        //[Authorize(Roles = "Admin")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<FlowResponseDto>> PostFlow(PostFlowRequestDto payload)
        {
            var result = await _flowService.CreateFlowAsync(payload);

            return Ok(new
            {
                Result = result,
                Success = true,
                Message = "Flow created succesfully.",
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PatchFlowAsync(Guid id, [FromBody] PatchFlowRequestDto payload)
        {
            var result = await _flowService.UpdateFlowAsync(id, payload);

            return Ok(new
            {
                Result = result,
                Success = true,
                Message = "Flow updated succesfully.",
                Timestamp = DateTime.UtcNow
            });
        }

        //[Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteFlowAsync(Guid id)
        {
            var result = await _flowService.DeleteFlowAsync(id);

            return Ok(new
            {
                Result = result,
                Success = true,
                Message = "Flow deleted succesfully.",
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpGet("{flowId}/steps")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetStepsForFlowAsync(Guid flowId)
        {
            var result = await _flowService.GetStepsForFlowAsync(flowId);

            if (result == null || !result.Any())
            {
                return NotFound(new
                {
                    Result = new List<StepResponseDto>(),
                    Message = "No steps found for the specified flow.",
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                Result = result,
                Success = true,
                Message = "Steps retreived succesfully.",
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpGet("flow-valid/{flowName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFlowNameUnicityAsync(string flowName)
        {
            var result = await _flowService.GetFlowNameUnicityAsync(flowName);

            return Ok(new
            {
                Result = result,
                Success = true,
                Message = "Steps unicity retrieved successfully.",
                Timestamp = DateTime.UtcNow,
            });
        }

        [HttpGet("by-form-template-id/{formTemplateId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFlowByFormTemplateIdAsync(Guid formTemplateId)
        {
            var result = await _flowService.GetFlowByFormTemplateIdAsync(formTemplateId);

            return Ok(new
            {
                Result = result,
                Success = true,
                Message = "Flow received successfully.",
                Timestamp = DateTime.UtcNow
            });
        }
    }
}