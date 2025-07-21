using FlowManager.Application.Interfaces;
using FlowManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlowManager.Infrastructure.Services
{
    public class FormService : IFormService
    {
        private readonly AppDbContext _context;

        public FormService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Form>> GetAllFormsAsync()
        {
            return await _context.Forms
                .Include(f => f.Flow)
                .Include(f => f.User)
                .Include(f => f.LastStep)
                .ToListAsync();
        }

        public async Task<Form?> GetFormByIdAsync(Guid id)
        {
            return await _context.Forms
                .Include(f => f.Flow)
                .Include(f => f.User)
                .Include(f => f.LastStep)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<IEnumerable<Form>> GetFormsByFlowAsync(Guid flowId)
        {
            return await _context.Forms
                .Include(f => f.Flow)
                .Include(f => f.User)
                .Include(f => f.LastStep)
                .Where(f => f.FlowId == flowId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Form>> GetFormsByUserAsync(Guid userId)
        {
            return await _context.Forms
                .Include(f => f.Flow)
                .Include(f => f.User)
                .Include(f => f.LastStep)
                .Where(f => f.UserId == userId)
                .ToListAsync();
        }

        public async Task<Form> CreateFormAsync(Form form)
        {
            form.Id = Guid.NewGuid();
            form.CreatedAt = DateTime.UtcNow;
            form.UpdatedAt = DateTime.UtcNow;

            _context.Forms.Add(form);
            await _context.SaveChangesAsync();
            return form;
        }

        public async Task<bool> UpdateFormAsync(Guid id, Form form)
        {
            if (id != form.Id)
                return false;

            form.UpdatedAt = DateTime.UtcNow;
            _context.Entry(form).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                return _context.Forms.Any(e => e.Id == id);
            }
        }

        public async Task<bool> DeleteFormAsync(Guid id)
        {
            var form = await _context.Forms.FindAsync(id);
            if (form == null)
                return false;

            _context.Forms.Remove(form);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
