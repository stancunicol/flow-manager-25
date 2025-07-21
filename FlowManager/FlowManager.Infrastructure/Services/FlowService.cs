using FlowManager.Application.Interfaces;
using FlowManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace FlowManager.Infrastructure.Services
{
    public class FlowService : IFlowService
    {
        private readonly AppDbContext _context;

        public FlowService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Flow>> GetAllFlowsAsync()
        {
            return await _context.Flows
                .Include(f => f.Steps)
                .Include(f => f.Forms)
                .ToListAsync();
        }

        public async Task<Flow?> GetFlowByIdAsync(Guid id)
        {
            return await _context.Flows
                .Include(f => f.Steps)
                .Include(f => f.Forms)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<Flow> CreateFlowAsync(Flow flow)
        {
            flow.Id = Guid.NewGuid();
            flow.CreatedAt = DateTime.UtcNow;
            flow.UpdatedAt = DateTime.UtcNow;

            _context.Flows.Add(flow);
            await _context.SaveChangesAsync();

            return flow;
        }

        public async Task<bool> UpdateFlowAsync(Guid id, Flow flow)
        {
            if (id != flow.Id)
                return false;

            flow.UpdatedAt = DateTime.UtcNow;
            _context.Entry(flow).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                return _context.Flows.Any(e => e.Id == id);
            }
        }

        public async Task<bool> DeleteFlowAsync(Guid id)
        {
            var flow = await _context.Flows.FindAsync(id);
            if (flow == null)
                return false;

            _context.Flows.Remove(flow);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
