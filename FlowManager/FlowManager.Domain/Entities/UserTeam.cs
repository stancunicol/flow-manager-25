
namespace FlowManager.Domain.Entities
{
    public class UserTeam
    {
        public virtual User User { get; set; } = null!;
        public Guid UserId { get; set; } 

        public virtual Team Team { get; set; } = null!;
        public Guid TeamId { get; set; }    

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
