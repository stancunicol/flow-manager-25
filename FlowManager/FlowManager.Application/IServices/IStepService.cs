using FlowManager.Shared.DTOs.Requests.Step;
using FlowManager.Shared.DTOs.Responses;
using FlowManager.Shared.DTOs.Responses.Step;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Application.Interfaces
{
    public interface IStepService
    {
        Task<PagedResponseDto<StepResponseDto>> GetAllStepsQueriedAsync(QueriedStepRequestDto payload);
        Task<StepResponseDto> GetStepAsync(Guid id);
        Task<List<StepResponseDto>> GetStepsAsync();
        Task<StepResponseDto> PostStepAsync(PostStepRequestDto payload);
        Task<StepResponseDto> PatchStepAsync(Guid id, PatchStepRequestDto payload);
        Task<StepResponseDto> DeleteStepAsync(Guid id);
        Task<StepResponseDto> UnassignUserFromStepAsync(Guid id, Guid userId);
        Task<StepResponseDto> AssignUserToStepAsync(Guid id, Guid userId);
    }
}
