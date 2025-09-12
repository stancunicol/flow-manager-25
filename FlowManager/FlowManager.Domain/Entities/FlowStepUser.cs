
namespace FlowManager.Domain.Entities
{
    public class FlowStepUser
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid FlowStepId { get; set; }
        public Guid UserId { get; set; }

        public virtual FlowStep FlowStep { get; set; } = null!;
        public virtual User User { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;  
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
