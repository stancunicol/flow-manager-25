namespace FlowManager.Client.ViewModels
{
    public class FormElement
    {
        public string Id { get; set; } = "";
        public int X { get; set; }
        public int Y { get; set; }
        public int ZIndex { get; set; }
        public bool IsTextElement { get; set; }
        public string? TextContent { get; set; }
        public Guid? ComponentId { get; set; }
        public string? ComponentType { get; set; }
        public string? Label { get; set; }
        public bool? Required { get; set; }
        public Dictionary<string, object>? Properties { get; set; }
    }
}
