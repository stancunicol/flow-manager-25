// Infrastructure/Repositories/FormResponseRepository.cs
using FlowManager.Domain.Dtos;
using FlowManager.Domain.Entities;
using FlowManager.Domain.IRepositories;
using FlowManager.Infrastructure.Context;
using FlowManager.Infrastructure.Utils;
using Microsoft.EntityFrameworkCore;

namespace FlowManager.Infrastructure.Repositories
{
    public class FormResponseRepository : IFormResponseRepository
    {
        private readonly AppDbContext _context;

        public FormResponseRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(List<FormResponse> Data, int TotalCount)> GetAllFormResponsesQueriedAsync(
            Guid? formTemplateId,
            Guid? stepId,
            Guid? userId,
            string? searchTerm,
            DateTime? createdFrom,
            DateTime? createdTo,
            bool includeDeleted,
            QueryParams? parameters)
        {
            IQueryable<FormResponse> query = _context.FormResponses
                .Include(fr => fr.FormTemplate)
                .Include(fr => fr.Step)
                .Include(fr => fr.User)
                .AsQueryable();

            // Apply filters
            if (!includeDeleted)
            {
                query = query.Where(fr => fr.DeletedAt == null);
            }

            if (formTemplateId.HasValue)
            {
                query = query.Where(fr => fr.FormTemplateId == formTemplateId.Value);
            }

            if (stepId.HasValue)
            {
                query = query.Where(fr => fr.StepId == stepId.Value);
            }

            if (userId.HasValue)
            {
                query = query.Where(fr => fr.UserId == userId.Value);
            }

            if (createdFrom.HasValue)
            {
                query = query.Where(fr => fr.CreatedAt >= createdFrom.Value);
            }

            if (createdTo.HasValue)
            {
                query = query.Where(fr => fr.CreatedAt <= createdTo.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var search = searchTerm.ToUpper();
                query = query.Where(fr =>
                    fr.FormTemplate.Name.ToUpper().Contains(search) ||
                    fr.Step.Name.ToUpper().Contains(search) ||
                    fr.User.Name.ToUpper().Contains(search) ||
                    (fr.RejectReason != null && fr.RejectReason.ToUpper().Contains(search)));
            }

            int totalCount = await query.CountAsync();

            if (parameters == null)
            {
                var data = await query.ToListAsync();
                return (data, totalCount);
            }

            if (parameters.SortBy != null)
            {
                if (parameters.SortDescending is bool sortDesc)
                    query = query.ApplySorting<FormResponse>(parameters.SortBy, sortDesc);
                else
                    query = query.ApplySorting<FormResponse>(parameters.SortBy, false);
            }

            if (parameters.Page == null || parameters.Page < 0 ||
                parameters.PageSize == null || parameters.PageSize < 0)
            {
                List<FormResponse> data = await query.ToListAsync();
                return (data, totalCount);
            }
            else
            {
                List<FormResponse> data = await query.Skip((int)parameters.PageSize * ((int)parameters.Page - 1))
                                                   .Take((int)parameters.PageSize)
                                                   .ToListAsync();
                return (data, totalCount);
            }
        }

        public async Task<List<FormResponse>> GetAllFormResponsesAsync()
        {
            return await _context.FormResponses
                .Include(fr => fr.FormTemplate)
                .Include(fr => fr.Step)
                .Include(fr => fr.User)
                .Where(fr => fr.DeletedAt == null)
                .OrderByDescending(fr => fr.CreatedAt)
                .ToListAsync();
        }

        public async Task<FormResponse?> GetFormResponseByIdAsync(Guid id)
        {
            return await _context.FormResponses
                .Include(fr => fr.FormTemplate)
                .Include(fr => fr.Step)
                .Include(fr => fr.User)
                .Where(fr => fr.DeletedAt == null)
                .FirstOrDefaultAsync(fr => fr.Id == id);
        }

        public async Task<List<FormResponse>> GetFormResponsesByUserAsync(Guid userId)
        {
            return await _context.FormResponses
                .Include(fr => fr.FormTemplate)
                .Include(fr => fr.Step)
                .Include(fr => fr.User)
                .Where(fr => fr.UserId == userId && fr.DeletedAt == null)
                .OrderByDescending(fr => fr.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<FormResponse>> GetFormResponsesByStepAsync(Guid stepId)
        {
            return await _context.FormResponses
                .Include(fr => fr.FormTemplate)
                .Include(fr => fr.Step)
                .Include(fr => fr.User)
                .Where(fr => fr.StepId == stepId && fr.DeletedAt == null)
                .OrderByDescending(fr => fr.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<FormResponse>> GetFormResponsesByTemplateAsync(Guid formTemplateId)
        {
            return await _context.FormResponses
                .Include(fr => fr.FormTemplate)
                .Include(fr => fr.Step)
                .Include(fr => fr.User)
                .Where(fr => fr.FormTemplateId == formTemplateId && fr.DeletedAt == null)
                .OrderByDescending(fr => fr.CreatedAt)
                .ToListAsync();
        }

        public async Task<(List<FormResponse> data, int totalCount)> GetFormResponsesAssignedToModeratorAsync(
            Guid moderatorId,
            string? searchTerm = null,
            DateTime? createdFrom = null,
            DateTime? createdTo = null,
            bool includeDeleted = false,
            QueryParams? queryParams = null)
        {
            var query = _context.FormResponses
                .Include(fr => fr.FormTemplate)
                .Include(fr => fr.Step)
                    .ThenInclude(s => s.Users.Where(su => su.DeletedAt == null))
                .Include(fr => fr.Step)
                    .ThenInclude(s => s.Teams.Where(st => st.DeletedAt == null))
                        .ThenInclude(st => st.Team.Users.Where(ut => ut.DeletedAt == null))
                .Include(fr => fr.User)
                .Where(fr =>
                    // Doar formularele unde moderatorul este asignat la step-ul curent
                    fr.Step.Users.Any(su => su.UserId == moderatorId && su.DeletedAt == null) ||
                    fr.Step.Teams.Any(st => st.Team.Users.Any(ut => ut.UserId == moderatorId && ut.DeletedAt == null) && st.DeletedAt == null)
                );

            // Exclude deleted unless explicitly requested
            if (!includeDeleted)
            {
                query = query.Where(fr => fr.DeletedAt == null);
            }

            // Apply search filter
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(fr =>
                    (fr.FormTemplate != null && fr.FormTemplate.Name.Contains(searchTerm)) ||
                    (fr.Step != null && fr.Step.Name.Contains(searchTerm)) ||
                    (fr.User != null && (fr.User.Name.Contains(searchTerm) || fr.User.Email.Contains(searchTerm))) ||
                    (fr.RejectReason != null && fr.RejectReason.Contains(searchTerm))
                );
            }

            // Apply date filters
            if (createdFrom.HasValue)
            {
                query = query.Where(fr => fr.CreatedAt >= createdFrom.Value);
            }

            if (createdTo.HasValue)
            {
                query = query.Where(fr => fr.CreatedAt <= createdTo.Value);
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            //// Apply sorting
            //query = ApplySorting(query, queryParams);

            //// Apply pagination
            //query = ApplyPagination(query, queryParams);

            var data = await query.ToListAsync();

            return (data, totalCount);
        }

        public async Task<List<FormResponse>> GetPendingFormResponsesAsync()
        {
            return await _context.FormResponses
                .Include(fr => fr.FormTemplate)
                .Include(fr => fr.Step)
                .Include(fr => fr.User)
                .Where(fr => fr.DeletedAt == null && fr.RejectReason == null)
                .OrderByDescending(fr => fr.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<FormResponse>> GetRejectedFormResponsesAsync()
        {
            return await _context.FormResponses
                .Include(fr => fr.FormTemplate)
                .Include(fr => fr.Step)
                .Include(fr => fr.User)
                .Where(fr => fr.DeletedAt == null && fr.RejectReason != null)
                .OrderByDescending(fr => fr.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<FormResponse>> GetFormResponsesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.FormResponses
                .Include(fr => fr.FormTemplate)
                .Include(fr => fr.Step)
                .Include(fr => fr.User)
                .Where(fr => fr.DeletedAt == null &&
                            fr.CreatedAt >= startDate &&
                            fr.CreatedAt <= endDate)
                .OrderByDescending(fr => fr.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> HasUserSubmittedResponseAsync(Guid userId, Guid formTemplateId, Guid stepId)
        {
            return await _context.FormResponses
                .AnyAsync(fr => fr.UserId == userId &&
                               fr.FormTemplateId == formTemplateId &&
                               fr.StepId == stepId &&
                               fr.DeletedAt == null);
        }

        public async Task<List<FormResponse>> GetStuckFormResponsesAsync(Guid stepId, int daysStuck = 7)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysStuck);

            return await _context.FormResponses
                .Include(fr => fr.FormTemplate)
                .Include(fr => fr.Step)
                .Include(fr => fr.User)
                .Where(fr => fr.StepId == stepId &&
                            fr.DeletedAt == null &&
                            fr.CreatedAt <= cutoffDate &&
                            fr.RejectReason == null)
                .OrderBy(fr => fr.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> BulkMoveToNextStepAsync(List<Guid> formResponseIds, Guid nextStepId)
        {
            var formResponses = await _context.FormResponses
                .Where(fr => formResponseIds.Contains(fr.Id) && fr.DeletedAt == null)
                .ToListAsync();

            foreach (var formResponse in formResponses)
            {
                formResponse.StepId = nextStepId;
                formResponse.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return formResponses.Count;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task AddAsync(FormResponse formResponse)
        {
            _context.FormResponses.Add(formResponse);
            await SaveChangesAsync();
        }

        public async Task UpdateAsync(FormResponse formResponse)
        {
            formResponse.UpdatedAt = DateTime.UtcNow;
            _context.FormResponses.Update(formResponse);
            await SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var formResponse = await _context.FormResponses
                .FirstOrDefaultAsync(fr => fr.Id == id && fr.DeletedAt == null);

            if (formResponse != null)
            {
                formResponse.DeletedAt = DateTime.UtcNow;
                await SaveChangesAsync();
            }
        }
    }
}