using FlowManager.Domain.Entities;

namespace FlowManager.Client.ViewModels
{
    public class FlowStepItemVM
    {
        public Guid? Id { get; set; }

        public FlowStepVM? FlowStep { get; set; }
        public Guid? FlowStepId { get; set; }

        public StepVM? Step { get; set; }
        public Guid? StepId { get; set; }

        public List<FlowStepItemUserVM> AssignedUsers { get; set; } = new List<FlowStepItemUserVM>();
        public List<FlowStepItemTeamVM> AssignedTeams { get; set; } = new List<FlowStepItemTeamVM>();

        public int? Order { get; set; } = 0;
    }
}
