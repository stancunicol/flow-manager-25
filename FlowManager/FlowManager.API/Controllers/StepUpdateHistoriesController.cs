using Microsoft.AspNetCore.Mvc;
using FlowManager.Application.Interfaces;
using FlowManager.Domain.Entities;

namespace FlowManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StepUpdateHistoriesController : ControllerBase
    {
        //private readonly IStepUpdateHistoryService _service;

        //public StepUpdateHistoriesController(IStepUpdateHistoryService service)
        //{
        //    _service = service;
        //}

        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<StepUpdateHistory>>> GetStepUpdateHistories()
        //{
        //    var histories = await _service.GetStepUpdateHistories();
        //    return Ok(histories);
        //}

        //[HttpGet("{id}")]
        //public async Task<ActionResult<StepUpdateHistory>> GetStepUpdateHistory(Guid id)
        //{
        //    var history = await _service.GetStepUpdateHistory(id);
        //    if (history == null)
        //        return NotFound();

        //    return Ok(history);
        //}

        //[HttpGet("step/{stepId}")]
        //public async Task<ActionResult<IEnumerable<StepUpdateHistory>>> GetHistoriesByStep(Guid stepId)
        //{
        //    var histories = await _service.GetHistoriesByStep(stepId);
        //    return Ok(histories);
        //}

        //[HttpGet("user/{userId}")]
        //public async Task<ActionResult<IEnumerable<StepUpdateHistory>>> GetHistoriesByUser(Guid userId)
        //{
        //    var histories = await _service.GetHistoriesByUser(userId);
        //    return Ok(histories);
        //}

        //[HttpPost]
        //public async Task<ActionResult<StepUpdateHistory>> PostStepUpdateHistory(StepUpdateHistory stepUpdateHistory)
        //{
        //    var created = await _service.PostStepUpdateHistory(stepUpdateHistory);
        //    return CreatedAtAction(nameof(GetStepUpdateHistory), new { id = created.Id }, created);
        //}

        //[HttpPut("{id}")]
        //public async Task<IActionResult> PutStepUpdateHistory(Guid id, StepUpdateHistory stepUpdateHistory)
        //{
        //    var success = await _service.PutStepUpdateHistory(id, stepUpdateHistory);
        //    return success ? NoContent() : NotFound();
        //}

        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteStepUpdateHistory(Guid id)
        //{
        //    var success = await _service.DeleteStepUpdateHistory(id);
        //    return success ? NoContent() : NotFound();
        //}
    }
}
