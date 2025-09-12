using System.ComponentModel.DataAnnotations;

namespace FlowManager.Domain.Entities
{
    public class FormReview
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid FormResponseId { get; set; }
        public FormResponse FormResponse { get; set; } = null!;

        [Required]
        public Guid ReviewerId { get; set; }
        public User Reviewer { get; set; } = null!;

        [Required]
        public Guid StepId { get; set; }
        public Step Step { get; set; } = null!;

        [Required]
        [StringLength(20)]
        public string Action { get; set; } = null!;

        [StringLength(500)]
        public string? RejectReason { get; set; }

        public DateTime ReviewedAt { get; set; } = DateTime.UtcNow;

        // Impersonation properties
        public bool IsImpersonatedAction { get; set; } = false;
        public Guid? ImpersonatedByUserId { get; set; }
        
        [StringLength(255)]
        public string? ImpersonatedByUserName { get; set; }

        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}