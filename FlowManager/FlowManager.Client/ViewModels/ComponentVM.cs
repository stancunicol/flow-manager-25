using FlowManager.Domain.Entities;

namespace FlowManager.Client.ViewModels
{
    public class ComponentVM
    {
        public Guid Id { get; set; }

        public string? Type { get; set; }
        public string? Label { get; set; }
        public bool? Required { get; set; }
        public Dictionary<string, object>? Properties { get; set; }

        public List<FormTemplateComponentVM>? FormTemplates { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
