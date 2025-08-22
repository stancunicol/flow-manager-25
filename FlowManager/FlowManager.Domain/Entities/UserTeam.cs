using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Domain.Entities
{
    public class UserTeam
    {
        public virtual User User { get; set; }
        public Guid UserId { get; set; } 

        public virtual Team Team { get; set; }
        public Guid TeamId { get; set; }    

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
