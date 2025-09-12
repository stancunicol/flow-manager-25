using Microsoft.AspNetCore.Identity;

namespace FlowManager.Domain.Entities
{
    public class Role : IdentityRole<Guid>
    {
        public virtual ICollection<UserRole> Users { get; set; } = new List<UserRole>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
