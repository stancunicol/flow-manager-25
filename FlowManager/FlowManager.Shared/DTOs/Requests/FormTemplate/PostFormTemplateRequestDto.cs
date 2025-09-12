
namespace FlowManager.Shared.DTOs.Requests.FormTemplate
{
    public class PostFormTemplateRequestDto
    {
        public string Name { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public Guid? FlowId { get; set; } = null;
        public List<Guid> Components { get; set; } = new List<Guid>();
    }
}
