using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Domain.Entities
{
    public class FlowStepItemTeam
    {
        public Guid FlowStepItemId { get; set; } = Guid.NewGuid();
        public Guid TeamId { get; set; } = Guid.NewGuid();

        // navigation properties
        public virtual FlowStepItem FlowStepItem { get; set; } = null!;
        public virtual Team Team { get; set; } = null!; 

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
