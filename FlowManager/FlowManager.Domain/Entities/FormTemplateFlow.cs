
namespace FlowManager.Domain.Entities
{
    public class FormTemplateFlow
    {
        public Guid FormTemplateId { get; set; }
        public FormTemplate FormTemplate { get; set; } = new FormTemplate();
        public Guid FlowId { get; set; }
        public Flow Flow { get; set; } = new Flow();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
