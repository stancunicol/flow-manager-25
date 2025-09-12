using FlowManager.Shared.DTOs.Responses.StepHistory;
using FlowManager.Shared.DTOs.Requests.StepHistory;
using FlowManager.Shared.DTOs.Responses;

namespace FlowManager.Application.Interfaces
{
    public interface IStepHistoryService
    {
        Task<PagedResponseDto<StepHistoryResponseDto>> GetStepHistoriesQueriedAsync(QueriedStepHistoryRequestDto? payload);
        Task<IEnumerable<StepHistoryResponseDto>> GetAllAsync();
        Task<StepHistoryResponseDto> GetByIdAsync(Guid id);
        Task<StepHistoryResponseDto> CreateStepHistoryForNameChangeAsync(CreateStepHistoryRequestDto payload);
        Task<StepHistoryResponseDto> CreateStepHistoryForMoveUsersAsync(CreateStepHistoryRequestDto payload);
        Task<StepHistoryResponseDto> CreateStepHistoryForCreateDepartmentAsync(CreateStepHistoryRequestDto payload);
        Task<StepHistoryResponseDto> CreateStepHistoryForDeleteDepartmentAsync(CreateStepHistoryRequestDto payload);
    }
}