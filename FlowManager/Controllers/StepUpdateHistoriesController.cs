using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlowManager.Domain.Entities;
using FlowManager.Infrastructure;

namespace FlowManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StepUpdateHistoriesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public StepUpdateHistoriesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<StepUpdateHistory>>> GetStepUpdateHistories()
        {
            return await _context.StepUpdateHistories
                .Include(suh => suh.Step)
                .Include(suh => suh.User)
                .OrderByDescending(suh => suh.UpdatedAt)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<StepUpdateHistory>> GetStepUpdateHistory(Guid id)
        {
            var stepUpdateHistory = await _context.StepUpdateHistories
                .Include(suh => suh.Step)
                .Include(suh => suh.User)
                .FirstOrDefaultAsync(suh => suh.Id == id);

            if (stepUpdateHistory == null)
            {
                return NotFound();
            }

            return stepUpdateHistory;
        }

        [HttpGet("step/{stepId}")]
        public async Task<ActionResult<IEnumerable<StepUpdateHistory>>> GetHistoriesByStep(Guid stepId)
        {
            return await _context.StepUpdateHistories
                .Include(suh => suh.Step)
                .Include(suh => suh.User)
                .Where(suh => suh.StepId == stepId)
                .OrderByDescending(suh => suh.UpdatedAt)
                .ToListAsync();
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<StepUpdateHistory>>> GetHistoriesByUser(Guid userId)
        {
            return await _context.StepUpdateHistories
                .Include(suh => suh.Step)
                .Include(suh => suh.User)
                .Where(suh => suh.UserId == userId)
                .OrderByDescending(suh => suh.UpdatedAt)
                .ToListAsync();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutStepUpdateHistory(Guid id, StepUpdateHistory stepUpdateHistory)
        {
            if (id != stepUpdateHistory.Id)
            {
                return BadRequest();
            }

            _context.Entry(stepUpdateHistory).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StepUpdateHistoryExists(id))
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
        public async Task<ActionResult<StepUpdateHistory>> PostStepUpdateHistory(StepUpdateHistory stepUpdateHistory)
        {
            stepUpdateHistory.Id = Guid.NewGuid();
            stepUpdateHistory.UpdatedAt = DateTime.UtcNow;
            
            _context.StepUpdateHistories.Add(stepUpdateHistory);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetStepUpdateHistory", new { id = stepUpdateHistory.Id }, stepUpdateHistory);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStepUpdateHistory(Guid id)
        {
            var stepUpdateHistory = await _context.StepUpdateHistories.FindAsync(id);
            if (stepUpdateHistory == null)
            {
                return NotFound();
            }

            _context.StepUpdateHistories.Remove(stepUpdateHistory);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool StepUpdateHistoryExists(Guid id)
        {
            return _context.StepUpdateHistories.Any(e => e.Id == id);
        }
    }
}