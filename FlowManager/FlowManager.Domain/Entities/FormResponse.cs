using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Domain.Entities
{
    public class FormResponse
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [MaxLength(100)]
        public string? RejectReason { get; set; }
        public Dictionary<Guid, object> ResponseFields { get; set; }

        // navigation fields
        public virtual FormTemplate FormTemplate { get; set; }
        public Guid FormTemplateId { get; set; }

        public virtual Step Step { get; set; }
        public Guid StepId { get; set; }

        public virtual User User { get; set; }
        public Guid UserId { get; set; }

        // completed fields !!!

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
