namespace FlowManager.Client.DTOs.Chat
{
    public class ChatRequest
    {
        public string Message { get; set; } = string.Empty;
        public string SessionId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? UserId { get; set; }
        public string? UserContext { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class ChatResponse
    {
        public string Content { get; set; } = string.Empty;
        public bool IsMarkdown { get; set; } = false;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string SessionId { get; set; } = string.Empty;
        public bool IsSuccessful { get; set; } = true;
        public string? ErrorMessage { get; set; }
        public List<QuickAction>? SuggestedActions { get; set; }
    }

    public class QuickAction
    {
        public string Label { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public string ActionType { get; set; } = "message"; 
    }
}