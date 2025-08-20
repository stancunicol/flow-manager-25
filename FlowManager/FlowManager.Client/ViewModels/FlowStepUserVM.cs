using FlowManager.Domain.Entities;

namespace FlowManager.Client.ViewModels
{
    public class FlowStepUserVM
    {
        public Guid FlowStepId { get; set; }
        public Guid UserId { get; set; }

        public FlowStep FlowStep { get; set; }
        public User User { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
