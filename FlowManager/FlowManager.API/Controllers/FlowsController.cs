using Microsoft.AspNetCore.Mvc;
using FlowManager.Application.Interfaces;
using FlowManager.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using FlowManager.Application.DTOs.Responses.Flow;
using FlowManager.Application.DTOs.Responses.Step;

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

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<FlowResponseDto>>> GetFlows()
        {
            var result = await _flowService.GetAllFlowsAsync();
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
        public async Task<ActionResult<FlowResponseDto>> GetFlow(Guid id)
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

        //[Authorize(Roles = "Admin")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<FlowResponseDto>> PostFlow(Flow flow)
        {
            var result = await _flowService.CreateFlowAsync(flow);
            return Ok(new
            {
                Result = result,
                Success = true,
                Message = "Flow created succesfully.",
                Timestamp = DateTime.UtcNow
            });
        }

        //[Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PutFlow(Guid id, Flow flow)
        {
            var result = await _flowService.UpdateFlowAsync(id, flow);
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
        public async Task<IActionResult> DeleteFlow(Guid id)
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

        [HttpGet("{id}/steps")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<StepResponseDto>>> GetFlowSteps(Guid id)
        {
            var result = await _flowService.GetFlowByIdAsync(id);
            return Ok(new
            {
                Result = result,
                Success = true,
                Message = "Steps retreived succesfully.",
                Timestamp = DateTime.UtcNow
            });
        }
    }
}