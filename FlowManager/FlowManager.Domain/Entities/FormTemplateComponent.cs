
namespace FlowManager.Domain.Entities
{
    public class FormTemplateComponent
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public virtual FormTemplate FormTemplate { get; set; } = null!;
        public Guid FormTemplateId { get; set; }

        public virtual Component Component { get; set; } = null!;
        public Guid ComponentId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
