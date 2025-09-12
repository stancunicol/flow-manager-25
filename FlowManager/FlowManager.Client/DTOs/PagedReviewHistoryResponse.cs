using FlowManager.Shared.DTOs.Responses.FormReview;

namespace FlowManager.Client.DTOs
{
    public class PagedReviewHistoryResponse
    {
        public List<FormReviewResponseDto> Reviews { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public bool HasMore { get; set; }
    }
}
