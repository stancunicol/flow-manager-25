using FlowManager.Shared.DTOs.Requests.FormReview;
using FlowManager.Shared.DTOs.Responses;
using FlowManager.Shared.DTOs.Responses.FormReview;

namespace FlowManager.Application.Interfaces
{
    public interface IFormReviewService
    {
        Task<PagedResponseDto<FormReviewResponseDto>> GetReviewHistoryByModeratorAsync(Guid moderatorId, QueriedFormReviewRequestDto payload);
        Task<List<FormReviewResponseDto>> GetReviewHistoryByFormResponseAsync(Guid formResponseId);
    }
}