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

        // Order of the step in the flow (1, 2, 3, etc.)
        public int Order { get; set; } = 0;

        // navigation properties
        public virtual List<FlowStepUser> AssignedUsers { get; set; } = new List<FlowStepUser>();
        public virtual List<FlowStepTeam> AssignedTeams { get; set; } = new List<FlowStepTeam>();

        public virtual Flow Flow { get; set; } = null!;
        public Guid FlowId { get; set; }

        public virtual Step Step { get; set; } = null!;
        public Guid StepId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
