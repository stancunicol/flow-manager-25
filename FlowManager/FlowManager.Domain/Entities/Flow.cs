using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace FlowManager.Domain.Entities
{
    public class Flow
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        public virtual ICollection<FlowStep> FlowSteps { get; set; } = new List<FlowStep>();
        public virtual ICollection<Form> Forms { get; set; } = new List<Form>();
        
        // Helper property for backward compatibility with client code
        [NotMapped]
        public ICollection<Step> Steps => FlowSteps?.Select(fs => fs.Step).ToList() ?? new List<Step>();
    }
}
