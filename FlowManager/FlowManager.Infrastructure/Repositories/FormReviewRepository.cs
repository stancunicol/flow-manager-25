using FlowManager.Domain.Entities;
using FlowManager.Domain.IRepositories;
using FlowManager.Domain.Dtos;
using FlowManager.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace FlowManager.Infrastructure.Repositories
{
    public class FormReviewRepository : IFormReviewRepository
    {
        private readonly AppDbContext _context;

        public FormReviewRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<FormReview>> GetReviewHistoryByModeratorAsync(Guid moderatorId)
        {
            return await _context.FormReviews
                .Where(fr => fr.ReviewerId == moderatorId && fr.DeletedAt == null)
                .Include(fr => fr.FormResponse)
                    .ThenInclude(fr => fr.FormTemplate)
                .Include(fr => fr.FormResponse)
                    .ThenInclude(fr => fr.User)
                .Include(fr => fr.Step)
                .OrderByDescending(fr => fr.ReviewedAt)
                .ToListAsync();
        }

        public async Task<(List<FormReview> data, int totalCount)> GetReviewHistoryByModeratorPagedAsync(
            Guid moderatorId,
            string? searchTerm = null,
            QueryParams? queryParams = null)
        {
            var query = _context.FormReviews
                .Where(fr => fr.ReviewerId == moderatorId && fr.DeletedAt == null)
                .Include(fr => fr.FormResponse)
                    .ThenInclude(fr => fr.FormTemplate)
                .Include(fr => fr.FormResponse)
                    .ThenInclude(fr => fr.User)
                .Include(fr => fr.Step)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var search = searchTerm.ToLower();
                query = query.Where(fr =>
                    fr.FormResponse.FormTemplate.Name.ToLower().Contains(search) ||
                    fr.FormResponse.User.Name.ToLower().Contains(search) ||
                    fr.Step.Name.ToLower().Contains(search) ||
                    fr.Action.ToLower().Contains(search));
            }

            var totalCount = await query.CountAsync();

            // Apply sorting
            if (!string.IsNullOrEmpty(queryParams?.SortBy))
            {
                switch (queryParams.SortBy.ToLower())
                {
                    case "reviewedat":
                        query = queryParams.SortDescending == true
                            ? query.OrderByDescending(fr => fr.ReviewedAt)
                            : query.OrderBy(fr => fr.ReviewedAt);
                        break;
                    case "action":
                        query = queryParams.SortDescending == true
                            ? query.OrderByDescending(fr => fr.Action)
                            : query.OrderBy(fr => fr.Action);
                        break;
                    default:
                        query = query.OrderByDescending(fr => fr.ReviewedAt);
                        break;
                }
            }
            else
            {
                query = query.OrderByDescending(fr => fr.ReviewedAt);
            }

            // Apply pagination - CORECȚIA AICI
            if (queryParams?.Page > 0 && queryParams?.PageSize > 0)
            {
                var skip = (queryParams.Page.Value - 1) * queryParams.PageSize.Value; // Convertește explicit la int
                query = query.Skip(skip).Take(queryParams.PageSize.Value); // Convertește explicit la int
            }

            var data = await query.ToListAsync();

            return (data, totalCount);
        }

        public async Task<List<FormReview>> GetReviewHistoryByFormResponseAsync(Guid formResponseId)
        {
            return await _context.FormReviews
                .Where(fr => fr.FormResponseId == formResponseId && fr.DeletedAt == null)
                .Include(fr => fr.Reviewer)
                .Include(fr => fr.Step)
                .OrderBy(fr => fr.ReviewedAt)
                .ToListAsync();
        }

        public async Task<FormReview> AddAsync(FormReview formReview)
        {
            _context.FormReviews.Add(formReview);
            await _context.SaveChangesAsync();
            return formReview;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}