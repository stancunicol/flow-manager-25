using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Domain.Entities
{
    public class FlowStep
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public bool? IsApproved { get; set; } = null;
        public int Order { get; set; } = 0;

        // navigation properties

        public virtual Flow Flow { get; set; } = null!;
        public Guid FlowId { get; set; }

        public virtual List<FlowStepItem> FlowStepItems { get; set; } = new List<FlowStepItem>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
