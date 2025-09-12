
namespace FlowManager.Shared.DTOs.Responses.FormTemplateComponent
{
    public class FormTemplateComponentResponseDto
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? Type { get; set; }
        public string? Label { get; set; }
        public bool? Required { get; set; }
        public Dictionary<string, object>? Properties { get; set; }
    }
}
