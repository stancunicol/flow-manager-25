
namespace FlowManager.Domain.Entities
{
    public class FlowStep
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public bool? IsApproved { get; set; } = null;
        public int Order { get; set; } = 0;

        public virtual List<FlowStepUser> AssignedUsers { get; set; } = new List<FlowStepUser>();
        public virtual List<FlowStepTeam> AssignedTeams { get; set; } = new List<FlowStepTeam>();

        public virtual Flow Flow { get; set; } = null!;
        public Guid FlowId { get; set; }

        public virtual Step Step { get; set; } = null!;
        public Guid StepId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
