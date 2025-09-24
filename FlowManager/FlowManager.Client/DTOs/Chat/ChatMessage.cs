namespace FlowManager.Client.DTOs.Chat
{
    public class ChatMessage
    {
        public string Content { get; set; } = string.Empty;
        public bool IsFromUser { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsMarkdown { get; set; } = false;
        public string MessageId { get; set; } = Guid.NewGuid().ToString();
        public MessageType Type { get; set; } = MessageType.Text;
    }

    public enum MessageType
    {
        Text,
        System,
        Error,
        Welcome,
        QuickAction
    }
}