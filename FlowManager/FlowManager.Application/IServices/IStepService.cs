using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowManager.Application.DTOs.Requests.Step;
using FlowManager.Application.DTOs.Responses;
using FlowManager.Application.DTOs.Responses.Step;
using FlowManager.Domain.Entities;

namespace FlowManager.Application.Interfaces
{
    public interface IStepService
    {
        Task<PagedResponseDto<StepResponseDto>> GetAllStepsQueriedAsync(QueriedStepRequestDto payload);
        Task<StepResponseDto> GetStepAsync(Guid id);
        Task<StepResponseDto> PostStepAsync(PostStepRequestDto payload);
        Task<StepResponseDto> PatchStepAsync(Guid id, PatchStepRequestDto payload);
        Task<StepResponseDto> DeleteStepAsync(Guid id);
        Task<StepResponseDto> AssignUserToStepAsync(Guid id, Guid userId);
        Task<StepResponseDto> UnassignUserFromStepAsync(Guid id, Guid userId);
        Task<StepResponseDto> AddStepToFlowAsync(Guid stepId, Guid flowId);
        Task<StepResponseDto> RemoveStepFromFlowAsync(Guid stepId, Guid flowId);
        Task<StepResponseDto> RestoreStepToFlowAsync(Guid stepId, Guid flowId);
    }
}
