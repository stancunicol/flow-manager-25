
namespace FlowManager.Domain.Entities
{
    public class FlowStepTeam
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid FlowStepId { get; set; }
        public Guid TeamId { get; set; }

        public virtual FlowStep FlowStep { get; set; } = null!;
        public virtual Team Team { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
