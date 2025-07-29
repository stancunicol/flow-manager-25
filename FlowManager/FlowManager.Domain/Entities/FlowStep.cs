using System;
using System.Text.Json.Serialization;

namespace FlowManager.Domain.Entities
{
    public class FlowStep
    {
        public Guid FlowId { get; set; }
        public Guid StepId { get; set; }
        public int Order { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public virtual Flow Flow { get; set; } = null!;
        public virtual Step Step { get; set; } = null!;
    }
}