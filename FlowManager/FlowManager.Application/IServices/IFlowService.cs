using FlowManager.Application.DTOs.Requests.Flow;
using FlowManager.Application.DTOs.Responses;
using FlowManager.Application.DTOs.Responses.Flow;
using FlowManager.Application.DTOs.Responses.Step;
using FlowManager.Domain.Entities;

namespace FlowManager.Application.Interfaces
{
    public interface IFlowService
    {
        Task<PagedResponseDto<FlowResponseDto>> GetAllFlowsQueriedAsync(QueriedFlowRequestDto payload);
        Task<FlowResponseDto> GetFlowByIdAsync(Guid id);
        Task<FlowResponseDto> CreateFlowAsync(PostFlowRequestDto payload);
        Task<FlowResponseDto> UpdateFlowAsync(Guid id, PatchFlowRequestDto payload);
        Task<FlowResponseDto> DeleteFlowAsync(Guid id);
        Task<List<StepResponseDto>> GetStepsForFlowAsync(Guid flowId);
    }
}