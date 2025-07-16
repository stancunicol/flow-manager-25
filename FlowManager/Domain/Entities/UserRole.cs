using System;

namespace FlowManager.Domain.Entities
{
    public enum RoleType
    {
        Basic,
        Moderator,
        Admin
    }
    
    public class UserRole
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public RoleType Role { get; set; }
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        
        public virtual User User { get; set; } = null!;
    }
}