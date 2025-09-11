using FlowManager.Domain.Entities;
using FlowManager.Domain.IRepositories;
using FlowManager.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using FlowManager.Shared.DTOs.Requests.StepHistory;
using FlowManager.Shared.DTOs.Responses.StepHistory;
using FlowManager.Shared.DTOs.Responses;

namespace FlowManager.Infrastructure.Repositories
{
    public class StepHistoryRepository : IStepHistoryRepository
    {
        private readonly AppDbContext _context;

        public StepHistoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResponseDto<StepHistoryResponseDto>> GetStepHistoriesQueriedAsync(QueriedStepHistoryRequestDto payload)
        {
            var query = _context.StepHistory.Include(s => s.Step).AsQueryable();

            if (payload.StepId.HasValue)
            {
                query = query.Where(sh => sh.StepId == payload.StepId.Value);
            }

            var totalItems = await query.CountAsync();

            if (payload.QueryParams != null)
            {
                if (!string.IsNullOrEmpty(payload.QueryParams.SortBy))
                {
                    if (payload.QueryParams.SortBy.Equals("DateTime", StringComparison.OrdinalIgnoreCase))
                    {
                        query = payload.QueryParams.SortBy == "desc"
                            ? query.OrderByDescending(sh => sh.DateTime)
                            : query.OrderBy(sh => sh.DateTime);
                    }
                    else if (payload.QueryParams.SortBy.Equals("Action", StringComparison.OrdinalIgnoreCase))
                    {
                        query = payload.QueryParams.SortBy == "desc"
                            ? query.OrderByDescending(sh => sh.Action)
                            : query.OrderBy(sh => sh.Action);
                    }
                }

                if (payload.QueryParams.Page.HasValue && payload.QueryParams.PageSize.HasValue)
                {
                    var skip = (payload.QueryParams.Page.Value - 1) * payload.QueryParams.PageSize.Value;
                    query = query.Skip(skip).Take(payload.QueryParams.PageSize.Value);
                }
            }

            var items = await query.Select(sh => new StepHistoryResponseDto
            {
                StepId = sh.StepId,
                StepName = sh.Step.Name,
                Action = sh.Action,
                Details = sh.Details,
                DateTime = sh.DateTime
            }).ToListAsync();

            return new PagedResponseDto<StepHistoryResponseDto>
            {
                Page = payload?.QueryParams?.Page ?? 1,
                PageSize = payload?.QueryParams?.PageSize ?? totalItems
            };
        }

        public async Task<IEnumerable<StepHistory>> GetAllAsync()
        {
            return await _context.StepHistory.Include(s => s.Step).ToListAsync();
        }

        public async Task<StepHistory> GetByIdAsync(Guid id)
        {
            return await _context.StepHistory.Include(s => s.Step)
                .FirstOrDefaultAsync(s => s.IdStepHistory == id);
        }

        public async Task CreateAsync(StepHistory stepHistory)
        {
            _context.StepHistory.Add(stepHistory);
            await _context.SaveChangesAsync();
        }
    }
}