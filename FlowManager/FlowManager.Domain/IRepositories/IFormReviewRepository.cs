using FlowManager.Domain.Entities;
using FlowManager.Domain.Dtos;

namespace FlowManager.Domain.IRepositories
{
    public interface IFormReviewRepository
    {
        Task<List<FormReview>> GetReviewHistoryByModeratorAsync(Guid moderatorId);
        Task<(List<FormReview> data, int totalCount)> GetReviewHistoryByModeratorPagedAsync(
            Guid moderatorId,
            string? searchTerm = null,
            QueryParams? queryParams = null);
        Task<List<FormReview>> GetReviewHistoryByFormResponseAsync(Guid formResponseId);
        Task<FormReview> AddAsync(FormReview formReview);
        Task SaveChangesAsync();
    }
}