using FlowManager.Client.ViewModels.Team;
using FlowManager.Domain.Entities;

namespace FlowManager.Client.ViewModels
{
    public class FlowStepItemTeamVM
    {
        public Guid? FlowStepItemId { get; set; }
        public Guid? TeamId { get; set; }

        public FlowStepItemVM? FlowStepItem { get; set; }
        public TeamVM? Team { get; set; }
    }
}
