using System.ComponentModel.DataAnnotations;

namespace FlowManager.Domain.Entities
{
    public class FormTemplate
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;
        [MaxLength(1000000)]
        public string Content { get; set; } = string.Empty;

        public virtual ICollection<FormTemplateComponent> Components { get; set; } = new List<FormTemplateComponent>();
        public virtual ICollection<FormTemplateFlow> FormTemplateFlows { get; set; } = new List<FormTemplateFlow>();
        public Flow? ActiveFlow => FormTemplateFlows
            .Where(ft => ft.DeletedAt == null && ft.Flow.DeletedAt == null)
            .OrderByDescending(ft => ft.CreatedAt)
            .FirstOrDefault(formTemplateFlow => formTemplateFlow.FormTemplateId == this.Id)
            ?.Flow;
        public Guid? ActiveFlowId => ActiveFlow?.Id;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
