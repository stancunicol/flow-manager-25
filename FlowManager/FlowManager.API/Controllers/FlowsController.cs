using Microsoft.AspNetCore.Mvc;
using FlowManager.Application.Interfaces;
using FlowManager.Domain.Entities;
using Microsoft.AspNetCore.Authorization;

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
        public async Task<ActionResult<IEnumerable<Flow>>> GetFlows()
        {
            var flows = await _flowService.GetAllFlowsAsync();
            return Ok(flows);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Flow>> GetFlow(Guid id)
        {
            var flow = await _flowService.GetFlowByIdAsync(id);
            if (flow == null)
                return NotFound();

            return Ok(flow);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Flow>> PostFlow(Flow flow)
        {
            var created = await _flowService.CreateFlowAsync(flow);
            return CreatedAtAction(nameof(GetFlow), new { id = created.Id }, created);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFlow(Guid id, Flow flow)
        {
            var updated = await _flowService.UpdateFlowAsync(id, flow);
            if (!updated)
                return NotFound();

            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFlow(Guid id)
        {
            var deleted = await _flowService.DeleteFlowAsync(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }

        [HttpGet("{id}/steps")]
        public async Task<ActionResult<IEnumerable<Step>>> GetFlowSteps(Guid id)
        {
            var flow = await _flowService.GetFlowByIdAsync(id);
            if (flow == null)
                return NotFound();

            return Ok(flow.Steps.OrderBy(s => s.CreatedAt));
        }
    }
}