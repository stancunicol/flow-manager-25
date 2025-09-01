using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Domain.Entities
{
    public class Flow
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [MaxLength(50)]
        public string Name { get; set; }

        // navigation properties
        public virtual ICollection<FormTemplate> FormTemplates { get; set; } = new List<FormTemplate>();
        public virtual ICollection<FlowStep> Steps { get; set; } = new List<FlowStep>();
        public FormTemplate? ActiveFormTemplate => FormTemplates
            .Where(ft => ft.DeletedAt == null)
            .OrderByDescending(ft => ft.CreatedAt)
            .FirstOrDefault();
        public Guid? FormTemplateId => ActiveFormTemplate?.Id;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; } 
    }
}
