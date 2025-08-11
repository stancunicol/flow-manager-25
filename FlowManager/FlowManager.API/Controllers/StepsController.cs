using Microsoft.AspNetCore.Mvc;
using FlowManager.Application.Interfaces;
using FlowManager.Domain.Entities;

namespace FlowManager.API.Controllers
{
    //[Route("api/[controller]")]
    //[ApiController]
    //public class StepsController : ControllerBase
    //{
    //    private readonly IStepService _stepService;

    //    public StepsController(IStepService stepService)
    //    {
    //        _stepService = stepService;
    //    }

    //    [HttpGet]
    //    public async Task<ActionResult<IEnumerable<Step>>> GetSteps()
    //    {
    //        var steps = await _stepService.GetSteps();
    //        return Ok(steps);
    //    }

    //    [HttpGet("{id}")]
    //    public async Task<ActionResult<Step>> GetStep(Guid id)
    //    {
    //        var step = await _stepService.GetStep(id);
    //        if (step == null)
    //            return NotFound();

    //        return Ok(step);
    //    }

    //    [HttpGet("flow/{flowId}")]
    //    public async Task<ActionResult<IEnumerable<Step>>> GetStepsByFlow(Guid flowId)
    //    {
    //        var steps = await _stepService.GetStepsByFlow(flowId);
    //        return Ok(steps);
    //    }

    //    [HttpPost]
    //    public async Task<ActionResult<Step>> PostStep(Step step)
    //    {
    //        var created = await _stepService.PostStep(step);
    //        return CreatedAtAction(nameof(GetStep), new { id = created.Id }, created);
    //    }

    //    [HttpPut("{id}")]
    //    public async Task<IActionResult> PutStep(Guid id, Step step)
    //    {
    //        var updated = await _stepService.PutStep(id, step);
    //        if (!updated)
    //            return NotFound();

    //        return NoContent();
    //    }

    //    [HttpDelete("{id}")]
    //    public async Task<IActionResult> DeleteStep(Guid id)
    //    {
    //        var deleted = await _stepService.DeleteStep(id);
    //        if (!deleted)
    //            return NotFound();

    //        return NoContent();
    //    }

    //    [HttpPost("{id}/assign-user/{userId}")]
    //    public async Task<IActionResult> AssignUserToStep(Guid id, Guid userId)
    //    {
    //        var success = await _stepService.AssignUserToStep(id, userId);
    //        return success ? NoContent() : NotFound();
    //    }

    //    [HttpDelete("{id}/unassign-user/{userId}")]
    //    public async Task<IActionResult> UnassignUserFromStep(Guid id, Guid userId)
    //    {
    //        var success = await _stepService.UnassignUserFromStep(id, userId);
    //        return success ? NoContent() : NotFound();
    //    }

    //    [HttpPost("{stepId}/add-to-flow/{flowId}")]
    //    public async Task<IActionResult> AddStepToFlow(Guid stepId, Guid flowId, [FromQuery] int order = 0)
    //    {
    //        var success = await _stepService.AddStepToFlow(stepId, flowId, order);
    //        return success ? NoContent() : NotFound();
    //    }

    //    [HttpDelete("{stepId}/remove-from-flow/{flowId}")]
    //    public async Task<IActionResult> RemoveStepFromFlow(Guid stepId, Guid flowId)
    //    {
    //        var success = await _stepService.RemoveStepFromFlow(stepId, flowId);
    //        return success ? NoContent() : NotFound();
    //    }
    //}
}
