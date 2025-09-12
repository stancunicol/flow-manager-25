using FlowManager.Shared.DTOs.Responses.FormTemplate;

namespace FlowManager.Client.DTOs
{
    public class PagedTemplatesResponse
    {
        public List<FormTemplateResponseDto> Templates { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public bool HasMore { get; set; }
    }
}
