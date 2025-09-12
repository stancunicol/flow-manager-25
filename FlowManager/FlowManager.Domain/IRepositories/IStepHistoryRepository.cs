using FlowManager.Domain.Entities;
using FlowManager.Shared.DTOs.Responses;
using FlowManager.Shared.DTOs.Responses.StepHistory;
using FlowManager.Shared.DTOs.Requests.StepHistory;

namespace FlowManager.Domain.IRepositories
{
    public interface IStepHistoryRepository
    {
        Task<PagedResponseDto<StepHistoryResponseDto>> GetStepHistoriesQueriedAsync(QueriedStepHistoryRequestDto payload);
        Task<IEnumerable<StepHistory>> GetAllAsync();
        Task<StepHistory> GetByIdAsync(Guid id);
        Task CreateAsync(StepHistory stepHistory);
    }
}