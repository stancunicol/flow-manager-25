using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Domain.Entities
{
    public class Team
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        // navigation properties
        public virtual ICollection<UserTeam> Users { get; set; } = new List<UserTeam>();
        public virtual ICollection<StepTeam> Steps { get; set; } = new List<StepTeam>();
        public virtual ICollection<FlowStepTeam> FlowStepTeams { get; set; } = new List<FlowStepTeam>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
