using FlowManager.Shared.DTOs.Requests.FormResponse;

namespace FlowManager.Client.DTOs
{
    public class PagedUserFormsResponse
    {
        public List<FormResponseResponseDto> FormResponses { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public bool HasMore { get; set; }
    }
}
