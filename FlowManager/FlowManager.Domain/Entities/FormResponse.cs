using System.ComponentModel.DataAnnotations;

namespace FlowManager.Domain.Entities
{
    public class FormResponse
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [MaxLength(100)]
        public string? RejectReason { get; set; }
        public string? Status { get; set; } = "Pending";
        public Dictionary<Guid, object> ResponseFields { get; set; } = new Dictionary<Guid, object>();

        public bool CompletedByAdmin { get; set; } = false;
        [MaxLength(100)]
        public string? CompletedByAdminName { get; set; }

        public bool ApprovedByAdmin { get; set; } = false;
        [MaxLength(100)]
        public string? ApprovedByAdminName { get; set; }

        public virtual FormTemplate FormTemplate { get; set; } = null!;
        public Guid FormTemplateId { get; set; }

        public virtual Step Step { get; set; } = null!;
        public Guid StepId { get; set; }

        public virtual User User { get; set; } = null!;
        public Guid UserId { get; set; }

        public virtual User? CompletedByOtherUser { get; set; }
        public Guid? CompletedByOtherUserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
