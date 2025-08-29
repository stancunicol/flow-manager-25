// Domain/IRepositories/IFormResponseRepository.cs
using FlowManager.Domain.Dtos;
using FlowManager.Domain.Entities;

namespace FlowManager.Domain.IRepositories
{
    public interface IFormResponseRepository
    {
        Task<(List<FormResponse> Data, int TotalCount)> GetAllFormResponsesQueriedAsync(
            Guid? formTemplateId,
            Guid? stepId,
            Guid? userId,
            string? searchTerm,
            DateTime? createdFrom,
            DateTime? createdTo,
            bool includeDeleted,
            QueryParams parameters);
        Task<List<FormResponse>> GetAllFormResponsesAsync();
        Task<FormResponse?> GetFormResponseByIdAsync(Guid id);
        Task<List<FormResponse>> GetFormResponsesByUserAsync(Guid userId);
        Task<List<FormResponse>> GetFormResponsesByStepAsync(Guid stepId);
        Task<List<FormResponse>> GetFormResponsesByTemplateAsync(Guid formTemplateId);
        Task<(List<FormResponse> data, int totalCount)> GetFormResponsesAssignedToModeratorAsync(
            Guid moderatorId,
            string? searchTerm = null,
            DateTime? createdFrom = null,
            DateTime? createdTo = null,
            bool includeDeleted = false,
            QueryParams? queryParams = null);
        Task<List<FormResponse>> GetPendingFormResponsesAsync();
        Task<List<FormResponse>> GetRejectedFormResponsesAsync();
        Task<List<FormResponse>> GetFormResponsesByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<bool> HasUserSubmittedResponseAsync(Guid userId, Guid formTemplateId, Guid stepId);
        Task<List<FormResponse>> GetStuckFormResponsesAsync(Guid stepId, int daysStuck = 7);
        Task<Step?> GetStepWithFlowInfoAsync(Guid stepId);
        Task<bool> IsLastStepInFlowAsync(Guid stepId);
        Task<List<FormResponse>> GetFormResponsesByStatusAsync(string status);
        Task SaveChangesAsync();
        Task AddAsync(FormResponse formResponse);
        Task UpdateAsync(FormResponse formResponse);
        Task DeleteAsync(Guid id); // Soft delete
        Task<int> BulkMoveToNextStepAsync(List<Guid> formResponseIds, Guid nextStepId);
    }
}