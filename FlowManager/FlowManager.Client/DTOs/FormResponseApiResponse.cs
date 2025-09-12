using FlowManager.Shared.DTOs.Requests.FormResponse;
using FlowManager.Shared.DTOs.Responses;

namespace FlowManager.Client.DTOs
{
    public class FormResponseApiResponse
    {
        public PagedResponseDto<FormResponseResponseDto>? Result { get; set; }
        public bool Success { get; set; }
        public string? Message { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
