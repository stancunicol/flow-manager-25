using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Domain.Entities
{
    public class StepTeam
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // navigation properties
        public virtual Step Step { get; set; }
        public Guid StepId { get; set; }

        public virtual Team Team { get; set; }
        public Guid TeamId { get; set; }

        // optional permissions or responsibilities for this team at this step
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
