using FlowManager.Domain.Entities;
using FlowManager.Domain.IRepositories;
using FlowManager.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Infrastructure.Repositories
{
    public class FlowRepository : IFlowRepository
    {
        private readonly AppDbContext _context;

        public FlowRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<List<Flow>> GetAllFlowsAsync()
        {
            return await _context.Flows
                .Include(f => f.Steps)
                .Include(f => f.FormTemplate)
                .ToListAsync();
        }

        public async Task<Flow?> GetFlowByIdAsync(Guid id)
        {
            return await _context.Flows
                .Include(f => f.Steps)
                .Include(f => f.FormTemplate)
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
            await _context.SaveChangesAsync();
            return true;
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
