using FlowManager.Domain.Dtos;
using FlowManager.Domain.Entities;
using FlowManager.Domain.IRepositories;
using FlowManager.Infrastructure.Context;
using FlowManager.Infrastructure.Utils;
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
                .Include(f => f.FormTemplates.Where(ft => ft.DeletedAt == null))
                .ToListAsync();
        }

        public async Task<Flow?> GetFlowByIdAsync(Guid id)
        {
            return await _context.Flows
                .Include(f => f.Steps.Where(fs => fs.DeletedAt == null))
                    .ThenInclude(fs => fs.Step)
                .Include(f => f.FormTemplates.Where(ft => ft.DeletedAt == null))
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<Flow> CreateFlowAsync(Flow flow)
        {
            _context.Flows.Add(flow);
            await _context.SaveChangesAsync();
            return flow;
        }

        public async Task<(List<Flow> Data, int TotalCount)> GetAllFlowsQueriedAsync(string? name, QueryParams? parameters)
        {
            IQueryable<Flow> query = _context.Flows
                .Include(f => f.Steps)
                    .ThenInclude(fs => fs.Step)
                .Include(f => f.FormTemplates.Where(ft => ft.DeletedAt == null));

            // filtering
            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(f => f.Name.ToUpper().Contains(name.ToUpper()));
            }

            int totalCount = await query.CountAsync();

            if (parameters == null)
            {
                return (await query.ToListAsync(), totalCount);
            }

            // sorting
            if (!string.IsNullOrEmpty(parameters.SortBy))
            {
                if (parameters.SortDescending is bool sortDesc)
                {
                    query = query.ApplySorting<Flow>(parameters.SortBy, sortDesc);
                }
                else
                {
                    query = query.ApplySorting<Flow>(parameters.SortBy, false);
                }
            }

            // pagination
            if (parameters.Page == null || parameters.Page < 0 ||
                parameters.PageSize == null || parameters.PageSize < 0)
            {
                return (await query.ToListAsync(), totalCount);
            }
            else
            {
                return (await query
                    .Skip((parameters.Page.Value - 1) * parameters.PageSize.Value)
                    .Take(parameters.PageSize.Value)
                    .ToListAsync(), totalCount);
            }
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<Flow?> GetFlowIncludeDeletedStepsByIdAsync(Guid id)
        {
            return await _context.Flows
                .Include(f => f.Steps.Where(s => s.DeletedAt == null))
                    .ThenInclude(fs => fs.Step)
                .Include(f => f.FormTemplates.Where(ft => ft.DeletedAt == null))
                .FirstOrDefaultAsync(f => f.Id == id);
        }
    }
}