using FlowManager.Client.ViewModels.Team;
using FlowManager.Domain.Entities;

namespace FlowManager.Client.ViewModels
{
    public class FlowStepTeamVM
    {
        public Guid FlowStepId { get; set; }
        public Guid TeamId { get; set; }

        public virtual FlowStepVM FlowStep { get; set; }
        public virtual TeamVM Team { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
