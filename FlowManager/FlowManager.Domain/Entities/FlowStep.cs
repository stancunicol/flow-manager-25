using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Domain.Entities
{
    public class FlowStep
    {
        public bool? IsApproved { get; set; } = null;

        // navigation properties
        public virtual Flow Flow { get; set; } = new Flow();
        public Guid FlowId { get; set; }

        public virtual Step Step { get; set; } = new Step();
        public Guid StepId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
