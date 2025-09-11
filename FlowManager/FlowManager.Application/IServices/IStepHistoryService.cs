using FlowManager.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using FlowManager.Shared.DTOs.Responses.StepHistory;
using FlowManager.Shared.DTOs.Requests.StepHistory;
using FlowManager.Shared.DTOs.Responses;

namespace FlowManager.Application.Interfaces
{
    public interface IStepHistoryService
    {
        Task<PagedResponseDto<StepHistoryResponseDto>> GetStepHistoriesQueriedAsync(QueriedStepHistoryRequestDto payload);
        Task<IEnumerable<StepHistory>> GetAllAsync();
        Task<StepHistoryResponseDto> GetByIdAsync(Guid id);
        Task<StepHistoryResponseDto> CreateStepHistoryForNameChangeAsync(CreateStepHistoryRequestDto payload);
        Task<StepHistoryResponseDto> CreateStepHistoryForMoveUsersAsync(CreateStepHistoryRequestDto payload);
        Task<StepHistoryResponseDto> CreateStepHistoryForCreateDepartmentAsync(CreateStepHistoryRequestDto payload);
        Task<StepHistoryResponseDto> CreateStepHistoryForDeleteDepartmentAsync(CreateStepHistoryRequestDto payload);
    }
}