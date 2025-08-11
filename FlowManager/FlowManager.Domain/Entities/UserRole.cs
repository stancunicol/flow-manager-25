using System;
using Microsoft.AspNetCore.Identity;

namespace FlowManager.Domain.Entities
{
   
    public class UserRole : IdentityUserRole<Guid>
    {
        // navigation properties
        public virtual User User { get; set; } 
        public virtual Role Role { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}