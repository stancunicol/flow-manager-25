using System;
using System.Collections.Generic;

namespace FlowManager.Domain.Entities
{
    public class Flow
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        public virtual ICollection<Step> Steps { get; set; } = new List<Step>();
        public virtual ICollection<Form> Forms { get; set; } = new List<Form>();
    }
}
