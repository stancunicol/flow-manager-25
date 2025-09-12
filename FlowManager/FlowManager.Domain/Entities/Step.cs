using System.ComponentModel.DataAnnotations;

namespace FlowManager.Domain.Entities
{
    public class Step
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

    public virtual ICollection<FlowStep> FlowSteps { get; set; } = new List<FlowStep>();
    public virtual ICollection<User> Users { get; set; } = new List<User>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
