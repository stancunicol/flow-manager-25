using FlowManager.Application.DTOs.Requests.FormResponse;
using FlowManager.Application.DTOs.Responses;

namespace FlowManager.Application.Interfaces
{
    public interface IFormResponseService
    {
        Task<PagedResponseDto<FormResponseResponseDto>> GetAllFormResponsesQueriedAsync(QueriedFormResponseRequestDto payload);
        Task<List<FormResponseResponseDto>> GetAllFormResponsesAsync();
        Task<FormResponseResponseDto> GetFormResponseByIdAsync(Guid id);
        Task<FormResponseResponseDto> PostFormResponseAsync(PostFormResponseRequestDto payload);
        Task<FormResponseResponseDto> PatchFormResponseAsync(PatchFormResponseRequestDto payload);
        Task<FormResponseResponseDto> DeleteFormResponseAsync(Guid id);
        Task<List<FormResponseResponseDto>> GetFormResponsesByUserAsync(Guid userId);
        Task<List<FormResponseResponseDto>> GetFormResponsesByStepAsync(Guid stepId);
        Task<List<FormResponseResponseDto>> GetFormResponsesByTemplateAsync(Guid formTemplateId);
    }
}
