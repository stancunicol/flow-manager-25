namespace FlowManager.Client.ViewModels
{
    public class FormTemplateComponentVM
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? Type { get; set; }
        public string? Label { get; set; }
        public bool? Required { get; set; }
        public Dictionary<string, object>? Properties { get; set; }

        public FormTemplateVM? FormTemplate { get; set; }
        public ComponentVM? Component { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        
    }
}
