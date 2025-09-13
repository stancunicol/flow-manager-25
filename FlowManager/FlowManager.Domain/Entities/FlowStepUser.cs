using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Domain.Entities
{
    public class FlowStepUser
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid FlowStepId { get; set; }
        public Guid UserId { get; set; }

        // navigation properties
        public virtual FlowStep FlowStep { get; set; }
        public virtual User User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;  
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
