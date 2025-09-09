using FlowManager.Domain.Entities;
using FlowManager.Shared.DTOs.Requests.Flow;
using FlowManager.Shared.DTOs.Responses;
using FlowManager.Shared.DTOs.Responses.Flow;
using FlowManager.Shared.DTOs.Responses.Step;

namespace FlowManager.Application.Interfaces
{
    public interface IFlowService
    {
        Task<FlowResponseDto> GetFlowByFormTemplateIdAsync(Guid formTemplateId);
        Task<PagedResponseDto<FlowResponseDto>> GetAllFlowsQueriedAsync(QueriedFlowRequestDto payload);
        Task<FlowResponseDto> GetFlowByIdAsync(Guid id);
        Task<FlowResponseDto> CreateFlowAsync(PostFlowRequestDto payload);
        Task<FlowResponseDto> UpdateFlowAsync(Guid id, PatchFlowRequestDto payload);
        Task<FlowResponseDto> DeleteFlowAsync(Guid id);
        Task<List<StepResponseDto>> GetStepsForFlowAsync(Guid flowId);
        Task<FlowResponseDto> GetFlowByIdIncludeStepsAsync(Guid flowId);
    }
}