using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowManager.Shared.DTOs.Requests.Flow;
using FlowManager.Shared.DTOs.Responses.FlowStep;
using FlowManager.Shared.DTOs.Responses;
using FlowManager.Domain.Entities;
using FlowManager.Shared.DTOs.Requests.FlowStep;

namespace FlowManager.Application.IServices
{
    public interface IFlowStepService
    {
        Task<PagedResponseDto<FlowStepResponseDto>> GetAllFlowStepsQueriedAsync(QueriedFlowRequestDto payload);
        Task<List<FlowStepResponseDto>> GetAllFlowStepsAsync();
        Task<FlowStepResponseDto?> GetFlowStepByIdAsync(Guid id);
        Task<FlowStepResponseDto> CreateFlowStepAsync(PostFlowStepRequestDto flowStep);
        Task<FlowStepResponseDto> UpdateFlowStepAsync(Guid id, PatchFlowStepRequestDto payload);
        Task<FlowStepResponseDto> DeleteFlowStepAsync(Guid id);
    }
}
