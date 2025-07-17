using System;

namespace FlowManager.Domain.Entities
{
    public enum FormStatus
    {
        Submitted,
        Approved,
        Rejected
    }
    
    public class Form
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid? LastStepId { get; set; } 
        public FormStatus Status { get; set; } = FormStatus.Submitted;
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public Guid FlowId { get; set; }
        public Guid UserId { get; set; }
        
        public virtual Flow Flow { get; set; } = null!;
        public virtual User User { get; set; } = null!;
        public virtual Step? LastStep { get; set; }
    }
}
