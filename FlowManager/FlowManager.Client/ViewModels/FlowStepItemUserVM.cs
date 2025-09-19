using FlowManager.Domain.Entities;

namespace FlowManager.Client.ViewModels
{
    public class FlowStepItemUserVM
    {
        public Guid? FlowStepItemId { get; set; }
        public Guid? UserId { get; set; }

        public FlowStepItemVM? FlowStepItem { get; set; }
        public UserVM? User { get; set; } 
    }
}
