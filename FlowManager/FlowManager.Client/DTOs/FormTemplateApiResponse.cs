namespace FlowManager.Client.DTOs
{
    public class FormTemplateApiResponse<T>
    {
        public T? Result { get; set; }
        public bool Success { get; set; }
        public string? Message { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
