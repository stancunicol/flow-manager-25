using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Domain.Entities
{
    public class Component
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Type { get; set; }
        public string Label { get; set; }
        public bool Required { get; set; }
        public Dictionary<string, object> Properties { get; set; }

        // navigation properties
        public virtual ICollection<FormTemplateComponent> FormTemplates { get; set; } = new List<FormTemplateComponent>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
