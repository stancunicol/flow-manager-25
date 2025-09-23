using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Domain.Entities
{
    public class FlowStepItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public virtual FlowStep FlowStep { get; set; } = null!;
        public Guid FlowStepId { get;set; }

        public virtual Step Step { get; set; } = null!;
        public Guid StepId { get; set; }

        public virtual List<FlowStepItemUser> AssignedUsers { get; set; } = new List<FlowStepItemUser>();
        public virtual List<FlowStepItemTeam> AssignedTeams { get; set; } = new List<FlowStepItemTeam>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
