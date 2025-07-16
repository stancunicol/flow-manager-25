using System;
using System.Collections.Generic;

namespace FlowManager.Domain.Entities
{
    
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        public virtual ICollection<Form> Forms { get; set; } = new List<Form>();
        public virtual ICollection<Step> AssignedSteps { get; set; } = new List<Step>();
        public virtual ICollection<StepUpdateHistory> UpdateHistories { get; set; } = new List<StepUpdateHistory>();
    }
}
