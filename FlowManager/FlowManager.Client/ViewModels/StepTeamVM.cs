using FlowManager.Client.ViewModels.Team;
using FlowManager.Domain.Entities;

namespace FlowManager.Client.ViewModels
{
    public class StepTeamVM
    {
        public Guid Id { get; set; }

        public StepVM? Step { get; set; }
        public TeamVM? Team { get; set; }

        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
