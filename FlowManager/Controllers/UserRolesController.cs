using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlowManager.Domain.Entities;
using FlowManager.Infrastructure;

namespace FlowManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserRolesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserRolesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserRole>>> GetUserRoles()
        {
            return await _context.UserRoles
                .Include(ur => ur.User)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserRole>> GetUserRole(Guid id)
        {
            var userRole = await _context.UserRoles
                .Include(ur => ur.User)
                .FirstOrDefaultAsync(ur => ur.Id == id);

            if (userRole == null)
            {
                return NotFound();
            }

            return userRole;
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<UserRole>>> GetRolesByUser(Guid userId)
        {
            return await _context.UserRoles
                .Include(ur => ur.User)
                .Where(ur => ur.UserId == userId)
                .ToListAsync();
        }

        [HttpGet("role/{roleType}")]
        public async Task<ActionResult<IEnumerable<UserRole>>> GetUsersByRole(RoleType roleType)
        {
            return await _context.UserRoles
                .Include(ur => ur.User)
                .Where(ur => ur.Role == roleType)
                .ToListAsync();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutUserRole(Guid id, UserRole userRole)
        {
            if (id != userRole.Id)
            {
                return BadRequest();
            }

            _context.Entry(userRole).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserRoleExists(id))
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
        public async Task<ActionResult<UserRole>> PostUserRole(UserRole userRole)
        {
            if (await _context.UserRoles.AnyAsync(ur => ur.UserId == userRole.UserId && ur.Role == userRole.Role))
            {
                return BadRequest("User already has this role");
            }

            userRole.Id = Guid.NewGuid();
            userRole.AssignedAt = DateTime.UtcNow;
            
            _context.UserRoles.Add(userRole);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUserRole", new { id = userRole.Id }, userRole);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserRole(Guid id)
        {
            var userRole = await _context.UserRoles.FindAsync(id);
            if (userRole == null)
            {
                return NotFound();
            }

            _context.UserRoles.Remove(userRole);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserRoleExists(Guid id)
        {
            return _context.UserRoles.Any(e => e.Id == id);
        }
    }
}