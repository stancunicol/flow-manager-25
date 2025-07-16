using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlowManager.Domain.Entities;
using FlowManager.Infrastructure;

namespace FlowManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StepsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public StepsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Step>>> GetSteps()
        {
            return await _context.Steps
                .Include(s => s.Flow)
                .Include(s => s.AssignedUsers)
                .Include(s => s.UpdateHistories)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Step>> GetStep(Guid id)
        {
            var step = await _context.Steps
                .Include(s => s.Flow)
                .Include(s => s.AssignedUsers)
                .Include(s => s.UpdateHistories)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (step == null)
            {
                return NotFound();
            }

            return step;
        }

        [HttpGet("flow/{flowId}")]
        public async Task<ActionResult<IEnumerable<Step>>> GetStepsByFlow(Guid flowId)
        {
            return await _context.Steps
                .Include(s => s.Flow)
                .Include(s => s.AssignedUsers)
                .Include(s => s.UpdateHistories)
                .Where(s => s.FlowId == flowId)
                .ToListAsync();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutStep(Guid id, Step step)
        {
            if (id != step.Id)
            {
                return BadRequest();
            }

            step.UpdatedAt = DateTime.UtcNow;
            _context.Entry(step).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StepExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<Step>> PostStep(Step step)
        {
            step.Id = Guid.NewGuid();
            step.CreatedAt = DateTime.UtcNow;
            step.UpdatedAt = DateTime.UtcNow;
            
            _context.Steps.Add(step);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetStep", new { id = step.Id }, step);
        }

        [HttpPost("{id}/assign-user/{userId}")]
        public async Task<IActionResult> AssignUserToStep(Guid id, Guid userId)
        {
            var step = await _context.Steps
                .Include(s => s.AssignedUsers)
                .FirstOrDefaultAsync(s => s.Id == id);
            
            if (step == null)
            {
                return NotFound("Step not found");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            if (!step.AssignedUsers.Any(u => u.Id == userId))
            {
                step.AssignedUsers.Add(user);
                await _context.SaveChangesAsync();
            }

            return NoContent();
        }

        [HttpDelete("{id}/unassign-user/{userId}")]
        public async Task<IActionResult> UnassignUserFromStep(Guid id, Guid userId)
        {
            var step = await _context.Steps
                .Include(s => s.AssignedUsers)
                .FirstOrDefaultAsync(s => s.Id == id);
            
            if (step == null)
            {
                return NotFound("Step not found");
            }

            var user = step.AssignedUsers.FirstOrDefault(u => u.Id == userId);
            if (user != null)
            {
                step.AssignedUsers.Remove(user);
                await _context.SaveChangesAsync();
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStep(Guid id)
        {
            var step = await _context.Steps.FindAsync(id);
            if (step == null)
            {
                return NotFound();
            }

            _context.Steps.Remove(step);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool StepExists(Guid id)
        {
            return _context.Steps.Any(e => e.Id == id);
        }
    }
}