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
            QueryParams? parameters,
            List<string>? statusFilters = null)
        {
            IQueryable<FormResponse> query = _context.FormResponses
                .Include(fr => fr.FormTemplate)
                .Include(fr => fr.FlowStep)
                    .ThenInclude(flowStep => flowStep.FlowStepItems)
                        .ThenInclude(flowStepItem => flowStepItem.Step)
                .Include(fr => fr.User)
                .Include(fr => fr.CompletedByOtherUser)
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

            if (stepId.HasValue && stepId != Guid.Empty)
            {
                query = query.Where(fr => fr.FlowStep.FlowStepItems.Any(flowStepItem => flowStepItem.Step.Id == stepId.Value));
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
                    fr.FlowStep.FlowStepItems.Any(flowStepItem => flowStepItem.Step.Name.ToUpper().Contains(search)) ||
                    fr.User.Name.ToUpper().Contains(search) ||
                    (fr.RejectReason != null && fr.RejectReason.ToUpper().Contains(search)));
            }

            if (statusFilters != null && statusFilters.Any())
            {
                query = query.Where(fr => statusFilters.Contains(fr.Status ?? "Pending"));
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
                .Include(fr => fr.FlowStep)
                    .ThenInclude(flowStep => flowStep.FlowStepItems)
                        .ThenInclude(flowStepItem => flowStepItem.Step)
                .Include(fr => fr.User)
                .Where(fr => fr.DeletedAt == null)
                .OrderByDescending(fr => fr.CreatedAt)
                .ToListAsync();
        }

        public async Task<FormResponse?> GetFormResponseByIdAsync(Guid id)
        {
            return await _context.FormResponses
                .Include(fr => fr.FormTemplate)
                .Include(fr => fr.FlowStep)
                    .ThenInclude(flowStep => flowStep.FlowStepItems)
                        .ThenInclude(flowStepItem => flowStepItem.Step)
                .Include(fr => fr.User)
                .Where(fr => fr.DeletedAt == null)
                .FirstOrDefaultAsync(fr => fr.Id == id);
        }

        public async Task<List<FormResponse>> GetFormResponsesByUserAsync(Guid userId)
        {
            return await _context.FormResponses
                .Include(fr => fr.FormTemplate)
                .Include(fr => fr.FlowStep)
                    .ThenInclude(flowStep => flowStep.FlowStepItems)
                        .ThenInclude(flowStepItem => flowStepItem.Step)
                .Include(fr => fr.User)
                .Where(fr => fr.UserId == userId && fr.DeletedAt == null)
                .OrderByDescending(fr => fr.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<FormResponse>> GetFormResponsesByFlowStepAsync(Guid flowStepId)
        {
            return await _context.FormResponses
                .Include(fr => fr.FormTemplate)
                .Include(fr => fr.FlowStep)
                    .ThenInclude(flowStep => flowStep.FlowStepItems)
                        .ThenInclude(flowStepItem => flowStepItem.Step)
                .Include(fr => fr.User)
                .Where(fr => fr.FlowStepId == flowStepId && fr.DeletedAt == null)
                .OrderByDescending(fr => fr.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<FormResponse>> GetFormResponsesByTemplateAsync(Guid formTemplateId)
        {
            return await _context.FormResponses
                .Include(fr => fr.FormTemplate)
                .Include(fr => fr.FlowStep)
                    .ThenInclude(flowStep => flowStep.FlowStepItems)
                        .ThenInclude(flowStepItem => flowStepItem.Step)
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
            var responses = await _context.FormResponses
                .Include(formResponse => formResponse.User)
                .Include(fr => fr.FlowStep)
                    .ThenInclude(flowStep => flowStep.FlowStepItems)
                        .ThenInclude(flowStepItem => flowStepItem.Step)
                .Include(formResponse => formResponse.FormTemplate)
                    .ThenInclude(formTemplate => formTemplate.FormTemplateFlows)
                        .ThenInclude(formTemplateFlow => formTemplateFlow.Flow)
                            .ThenInclude(flow => flow.FlowSteps)
                                .ThenInclude(flowStep => flowStep.FlowStepItems)
                                    .ThenInclude(flowStepItem => flowStepItem.AssignedUsers)
                .Include(fr => fr.FormTemplate)
                    .ThenInclude(formTemplate => formTemplate.FormTemplateFlows)
                        .ThenInclude(formTemplateFlow => formTemplateFlow.Flow)
                            .ThenInclude(flow => flow.FlowSteps)
                                .ThenInclude(flowStep => flowStep.FlowStepItems)
                                    .ThenInclude(flowStepItem => flowStepItem.AssignedTeams)
                                        .ThenInclude(flowStepTeam => flowStepTeam.Team)
                                            .ThenInclude(flowStepTeam => flowStepTeam.Users)
                .ToListAsync();

            responses = responses.Where(formResponse =>
                (formResponse.Status == null || formResponse.Status == "Pending") &&
                (formResponse.FormTemplate.ActiveFlow?.FlowSteps.Any(flowStep =>
                    flowStep.FlowStepItems.Any(flowStepItem => flowStepItem.AssignedUsers.Any(flowStepUser => flowStepUser.UserId == moderatorId)) ||
                    flowStep.FlowStepItems.Any(flowStepItem => flowStepItem.AssignedTeams.Any(flowStepTeam => flowStepTeam.Team.Users.Any(teamUser => teamUser.UserId == moderatorId)))
                ) ?? false)
            ).ToList();

            // Exclude deleted unless explicitly requested
            if (!includeDeleted)
            {
                responses = responses.Where(fr => fr.DeletedAt == null).ToList();
            }

            // Apply search filter
            if (!string.IsNullOrEmpty(searchTerm))
            {
                responses = responses.Where(fr =>
                    (fr.FormTemplate != null && fr.FormTemplate.Name.Contains(searchTerm)) ||
                    (fr.FlowStep != null && fr.FlowStep.FlowStepItems.Any(flowStepItem => flowStepItem.Step.Name.Contains(searchTerm))) ||
                    (fr.User != null && (!string.IsNullOrEmpty(fr.User.Name) && fr.User.Name.Contains(searchTerm) ||
                                        !string.IsNullOrEmpty(fr.User.Email) && fr.User.Email.Contains(searchTerm))) ||
                    (fr.RejectReason != null && fr.RejectReason.Contains(searchTerm))
                ).ToList();
            }

            // Apply date filters
            if (createdFrom.HasValue)
            {
                responses = responses.Where(fr => fr.CreatedAt >= createdFrom.Value).ToList();
            }

            if (createdTo.HasValue)
            {
                responses = responses.Where(fr => fr.CreatedAt <= createdTo.Value).ToList();
            }

            // Get total count before pagination
            var totalCount = responses.Count();

            //// Apply sorting
            //query = ApplySorting(query, queryParams);

            //// Apply pagination
            //query = ApplyPagination(query, queryParams);

            var data = responses.ToList();

            return (data, totalCount);
        }


        public async Task<Step?> GetStepWithFlowInfoAsync(Guid stepId)
        {
            return await _context.Steps
                .Include(s => s.FlowSteps)
                    .ThenInclude(s => s.FlowStepItems)
                        .ThenInclude(flowStepItem => flowStepItem.FlowStep)
                            .ThenInclude(fs => fs.Flow)
                .FirstOrDefaultAsync(s => s.Id == stepId && s.DeletedAt == null);
        }

        public async Task<List<FormResponse>> GetFormResponsesByStatusAsync(string status)
        {
            return await _context.FormResponses
                .Include(fr => fr.FormTemplate)
                .Include(fr => fr.FlowStep)
                    .ThenInclude(flowStep => flowStep.FlowStepItems)
                        .ThenInclude(flowStepItem => flowStepItem.Step)
                .Include(fr => fr.User)
                .Where(fr => fr.Status == status && fr.DeletedAt == null)
                .OrderByDescending(fr => fr.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<FormResponse>> GetPendingFormResponsesAsync()
        {
            return await _context.FormResponses
                .Include(fr => fr.FormTemplate)
                .Include(fr => fr.FlowStep)
                    .ThenInclude(flowStep => flowStep.FlowStepItems)
                        .ThenInclude(flowStepItem => flowStepItem.Step)
                .Include(fr => fr.User)
                .Where(fr => fr.DeletedAt == null && fr.RejectReason == null)
                .OrderByDescending(fr => fr.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<FormResponse>> GetRejectedFormResponsesAsync()
        {
            return await _context.FormResponses
                .Include(fr => fr.FormTemplate)
                .Include(fr => fr.FlowStep)
                    .ThenInclude(flowStep => flowStep.FlowStepItems)
                        .ThenInclude(flowStepItem => flowStepItem.Step)
                .Include(fr => fr.User)
                .Where(fr => fr.DeletedAt == null && fr.RejectReason != null)
                .OrderByDescending(fr => fr.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<FormResponse>> GetFormResponsesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.FormResponses
                .Include(fr => fr.FormTemplate)
                .Include(fr => fr.FlowStep)
                    .ThenInclude(flowStep => flowStep.FlowStepItems)
                        .ThenInclude(flowStepItem => flowStepItem.Step)
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
                               fr.FlowStep.FlowStepItems.Any(flowStepItem => flowStepItem.FlowStepId == stepId) &&
                               fr.DeletedAt == null);
        }

        public async Task<List<FormResponse>> GetStuckFormResponsesAsync(Guid stepId, int daysStuck = 7)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysStuck);

            return await _context.FormResponses
                .Include(fr => fr.FormTemplate)
                .Include(fr => fr.FlowStep)
                    .ThenInclude(flowStep => flowStep.FlowStepItems)
                        .ThenInclude(flowStepItem => flowStepItem.Step)
                .Include(fr => fr.User)
                .Where(fr => fr.FlowStep.FlowStepItems.Any(flowStepItem => flowStepItem.FlowStepId == stepId) &&
                            fr.DeletedAt == null &&
                            fr.CreatedAt <= cutoffDate &&
                            fr.RejectReason == null)
                .OrderBy(fr => fr.CreatedAt)
                .ToListAsync();
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