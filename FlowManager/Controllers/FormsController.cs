using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlowManager.Domain.Entities;
using FlowManager.Infrastructure;

namespace FlowManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FormsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FormsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Form>>> GetForms()
        {
            return await _context.Forms
                .Include(f => f.Flow)
                .Include(f => f.User)
                .Include(f => f.LastStep)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Form>> GetForm(Guid id)
        {
            var form = await _context.Forms
                .Include(f => f.Flow)
                .Include(f => f.User)
                .Include(f => f.LastStep)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (form == null)
            {
                return NotFound();
            }

            return form;
        }

        [HttpGet("flow/{flowId}")]
        public async Task<ActionResult<IEnumerable<Form>>> GetFormsByFlow(Guid flowId)
        {
            return await _context.Forms
                .Include(f => f.Flow)
                .Include(f => f.User)
                .Include(f => f.LastStep)
                .Where(f => f.FlowId == flowId)
                .ToListAsync();
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Form>>> GetFormsByUser(Guid userId)
        {
            return await _context.Forms
                .Include(f => f.Flow)
                .Include(f => f.User)
                .Include(f => f.LastStep)
                .Where(f => f.UserId == userId)
                .ToListAsync();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutForm(Guid id, Form form)
        {
            if (id != form.Id)
            {
                return BadRequest();
            }

            form.UpdatedAt = DateTime.UtcNow;
            _context.Entry(form).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FormExists(id))
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
        public async Task<ActionResult<Form>> PostForm(Form form)
        {
            form.Id = Guid.NewGuid();
            form.CreatedAt = DateTime.UtcNow;
            form.UpdatedAt = DateTime.UtcNow;
            
            _context.Forms.Add(form);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetForm", new { id = form.Id }, form);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteForm(Guid id)
        {
            var form = await _context.Forms.FindAsync(id);
            if (form == null)
            {
                return NotFound();
            }

            _context.Forms.Remove(form);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool FormExists(Guid id)
        {
            return _context.Forms.Any(e => e.Id == id);
        }
    }
}