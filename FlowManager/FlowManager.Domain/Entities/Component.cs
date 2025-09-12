
namespace FlowManager.Domain.Entities
{
    public class Component
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Type { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public bool Required { get; set; }
        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();

        public virtual ICollection<FormTemplateComponent> FormTemplates { get; set; } = new List<FormTemplateComponent>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
