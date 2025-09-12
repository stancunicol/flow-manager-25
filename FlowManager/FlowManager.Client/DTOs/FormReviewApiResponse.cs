using FlowManager.Shared.DTOs.Responses;
using FlowManager.Shared.DTOs.Responses.FormReview;

namespace FlowManager.Client.DTOs
{
    public class FormReviewApiResponse
    {
        public PagedResponseDto<FormReviewResponseDto>? Result { get; set; }
        public bool Success { get; set; }
        public string? Message { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
