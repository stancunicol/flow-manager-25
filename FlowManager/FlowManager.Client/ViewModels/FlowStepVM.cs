using FlowManager.Domain.Entities;

namespace FlowManager.Client.ViewModels
{
    public class FlowStepVM
    {
        public Guid Id { get; set; } = Guid.Empty;
        public bool? IsApproved { get; set; }

        // navigation properties
        public List<FlowStepUserVM>? AssignedUsers { get; set; }
        public List<FlowStepTeamVM>? AssignedTeams { get; set; }

        public FlowVM? Flow { get; set; }

        public StepVM? Step { get; set; } 

        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
