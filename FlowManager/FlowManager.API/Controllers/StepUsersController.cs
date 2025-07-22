using Microsoft.AspNetCore.Mvc;
using FlowManager.Application.Interfaces;
using FlowManager.Domain.Entities;

namespace FlowManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StepUsersController : ControllerBase
    {
        private readonly IStepUserService _service;

        public StepUsersController(IStepUserService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<StepUser>>> GetAll()
        {
            var list = await _service.GetAll();
            return Ok(list);
        }

        [HttpGet("step/{stepId}")]
        public async Task<ActionResult<IEnumerable<User>>> GetUsersByStep(Guid stepId)
        {
            var users = await _service.GetUsersByStep(stepId);
            return Ok(users);
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Step>>> GetStepsByUser(Guid userId)
        {
            var steps = await _service.GetStepsByUser(userId);
            return Ok(steps);
        }

        [HttpPost("assign")]
        public async Task<IActionResult> AssignUser([FromQuery] Guid stepId, [FromQuery] Guid userId)
        {
            var success = await _service.AssignUser(stepId, userId);
            return success ? NoContent() : BadRequest();
        }

        [HttpDelete("unassign")]
        public async Task<IActionResult> UnassignUser([FromQuery] Guid stepId, [FromQuery] Guid userId)
        {
            var success = await _service.UnassignUser(stepId, userId);
            return success ? NoContent() : NotFound();
        }
    }
}
