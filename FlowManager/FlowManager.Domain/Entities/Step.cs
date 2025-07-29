using System;
using System.Collections.Generic;

namespace FlowManager.Domain.Entities
{
    public class Step
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        public virtual ICollection<FlowStep> FlowSteps { get; set; } = new List<FlowStep>();
        public virtual ICollection<StepUser> StepUsers { get; set; } = new List<StepUser>();
        public virtual ICollection<StepUpdateHistory> UpdateHistories { get; set; } = new List<StepUpdateHistory>();
    }
}
