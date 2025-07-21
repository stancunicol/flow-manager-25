using System;
using Microsoft.AspNetCore.Identity;

namespace FlowManager.Domain.Entities
{
   
    public class UserRole : IdentityUserRole<Guid>
    {
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        
        public virtual User User { get; set; } = null!;
        public virtual IdentityRole<Guid> Role { get; set; } = null!;
    }
}