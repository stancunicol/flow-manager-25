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
        public string? Status { get; set; } = "Pending";
        public Dictionary<Guid, object> ResponseFields { get; set; }

        // Admin completion tracking
        public bool CompletedByAdmin { get; set; } = false;
        [MaxLength(100)]
        public string? CompletedByAdminName { get; set; }

        // Admin approval tracking removed - use FormReview for complete audit trail

        // navigation fields
        public virtual FormTemplate FormTemplate { get; set; }
        public Guid FormTemplateId { get; set; }

        public virtual Step Step { get; set; }
        public Guid StepId { get; set; }

        public virtual User User { get; set; }
        public Guid UserId { get; set; }

        public virtual User? CompletedByOtherUser { get; set; }
        public Guid? CompletedByOtherUserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
