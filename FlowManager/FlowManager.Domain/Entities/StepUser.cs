using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Domain.Entities
{
    public class StepUser
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // navigation properties
        public virtual Step Step { get; set; }
        public Guid StepId { get; set; }

        public virtual User User { get; set; }
        public Guid UserId { get; set; }

        // permissions for this user at this specific step

        // tracking assignment date
       
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
