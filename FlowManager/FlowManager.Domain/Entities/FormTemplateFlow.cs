using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Domain.Entities
{
    public class FormTemplateFlow
    {
        public Guid FormTemplateId { get; set; }
        public FormTemplate FormTemplate { get; set; }
        public Guid FlowId { get; set; }
        public Flow Flow { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
