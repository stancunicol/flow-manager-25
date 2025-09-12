using System.ComponentModel.DataAnnotations;

namespace FlowManager.Domain.Entities
{
    public class Flow
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        public virtual ICollection<FlowStep> Steps { get; set; } = new List<FlowStep>();
        public virtual ICollection<FormTemplateFlow> FormTemplateFlows { get; set; } = new List<FormTemplateFlow>();
        public FormTemplate? ActiveFormTemplate => FormTemplateFlows
            .Where(ft => ft.DeletedAt == null && ft.FormTemplate.DeletedAt == null)
            .OrderByDescending(ft => ft.CreatedAt)
            .FirstOrDefault(formTemplateFlow => formTemplateFlow.FlowId == this.Id)?.FormTemplate;
        public Guid? ActiveFormTemplateId => ActiveFormTemplate?.Id;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; } 
    }
}
