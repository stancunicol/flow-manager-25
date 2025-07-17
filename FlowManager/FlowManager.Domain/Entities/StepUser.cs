using System;

namespace FlowManager.Domain.Entities
{
    public class StepUser
    {
        public Guid StepId { get; set; }
        public Guid UserId { get; set; }
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        
        public virtual Step Step { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}