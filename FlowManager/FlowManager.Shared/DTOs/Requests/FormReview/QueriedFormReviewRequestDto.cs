
namespace FlowManager.Shared.DTOs.Requests.FormReview
{
    public class QueriedFormReviewRequestDto
    {
        public string? SearchTerm { get; set; }
        public string? Action { get; set; }
        public QueryParamsDto? QueryParams { get; set; }
    }
}