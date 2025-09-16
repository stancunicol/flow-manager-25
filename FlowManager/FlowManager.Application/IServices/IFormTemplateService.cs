using FlowManager.Shared.DTOs.Requests.FormTemplate;
using FlowManager.Shared.DTOs.Responses;
using FlowManager.Shared.DTOs.Responses.FormTemplate;

namespace FlowManager.Application.Interfaces
{
    public interface IFormTemplateService
    {
        Task<PagedResponseDto<FormTemplateResponseDto>> GetAllFormTemplatesQueriedAsync(QueriedFormTemplateRequestDto payload);
        Task<FormTemplateResponseDto> GetFormTemplateByIdAsync(Guid id);
        Task<FormTemplateResponseDto> PostFormTemplateAsync(PostFormTemplateRequestDto payload);
        Task<FormTemplateResponseDto> DeleteFormTemplateAsync(Guid id);
        Task<FormTemplateResponseDto> PatchFormTemplateAsync(Guid id,PatchFormTemplateRequestDto payload);
        Task<bool> GetFormTemplateNameUnicityAsync(string formTemplateName);
    }
}
