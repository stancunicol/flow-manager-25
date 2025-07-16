using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlowManager.Domain.Entities;
using FlowManager.Infrastructure;

namespace FlowManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlowsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FlowsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Flow>>> GetFlows()
        {
            return await _context.Flows
                .Include(f => f.Steps)
                .Include(f => f.Forms)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Flow>> GetFlow(Guid id)
        {
            var flow = await _context.Flows
                .Include(f => f.Steps)
                .Include(f => f.Forms)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (flow == null)
            {
                return NotFound();
            }

            return flow;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutFlow(Guid id, Flow flow)
        {
            if (id != flow.Id)
            {
                return BadRequest();
            }

            flow.UpdatedAt = DateTime.UtcNow;
            _context.Entry(flow).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FlowExists(id))
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
        public async Task<ActionResult<Flow>> PostFlow(Flow flow)
        {
            flow.Id = Guid.NewGuid();
            flow.CreatedAt = DateTime.UtcNow;
            flow.UpdatedAt = DateTime.UtcNow;
            
            _context.Flows.Add(flow);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetFlow", new { id = flow.Id }, flow);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFlow(Guid id)
        {
            var flow = await _context.Flows.FindAsync(id);
            if (flow == null)
            {
                return NotFound();
            }

            _context.Flows.Remove(flow);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool FlowExists(Guid id)
        {
            return _context.Flows.Any(e => e.Id == id);
        }
    }
}