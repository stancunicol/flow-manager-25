
namespace FlowManager.Client.DTOs
{
    public class ApiResponse<T>
    {
        public T Result { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
        public bool Success { get; set; }
    }
}
