
namespace FlowManager.Shared.DTOs.Responses.ApiResponse
{
    public class ApiResponseDto<T>
    {
        public T Result { get; set; } = default!;
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public bool Success { get; set; }
    }
}
