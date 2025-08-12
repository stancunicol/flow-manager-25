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
    }
}
