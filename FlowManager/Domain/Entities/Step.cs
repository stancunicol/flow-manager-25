using System;
using System.Collections.Generic;

namespace FlowManager.Domain.Entities
{
    public class Step
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public Guid FlowId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        public virtual Flow Flow { get; set; } = null!;
        public virtual ICollection<User> AssignedUsers { get; set; } = new List<User>();
        public virtual ICollection<StepUpdateHistory> UpdateHistories { get; set; } = new List<StepUpdateHistory>();
    }
}
