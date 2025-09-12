
namespace FlowManager.Shared.DTOs.Requests.FormTemplate
{
    public class PatchFormTemplateRequestDto
    {
        public string? Name { get; set; }
        public string? Content { get; set; }
        public List<Guid>? Components { get; set; }
    }
}
