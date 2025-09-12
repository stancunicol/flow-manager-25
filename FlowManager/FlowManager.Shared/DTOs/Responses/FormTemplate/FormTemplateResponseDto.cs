using FlowManager.Shared.DTOs.Responses.FormTemplateComponent;

namespace FlowManager.Shared.DTOs.Responses.FormTemplate
{
    public class FormTemplateResponseDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Content { get; set; }
        public List<FormTemplateComponentResponseDto>? Components { get; set; } = new List<FormTemplateComponentResponseDto>();
        public Guid? FlowId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
