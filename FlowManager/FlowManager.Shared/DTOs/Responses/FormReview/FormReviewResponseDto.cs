
namespace FlowManager.Shared.DTOs.Responses.FormReview
{
    public class FormReviewResponseDto
    {
        public Guid Id { get; set; }
        public Guid FormResponseId { get; set; }
        public Guid FormTemplateId { get; set; }
        public string? FormTemplateName { get; set; }
        public Dictionary<Guid, object> ResponseFields { get; set; } = new();
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
        public Guid ReviewerId { get; set; }
        public string? ReviewerName { get; set; }
        public Guid StepId { get; set; }
        public string? StepName { get; set; }
        public string Action { get; set; } = null!;
        public string? RejectReason { get; set; }
        public DateTime ReviewedAt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public bool IsImpersonatedAction { get; set; }
        public Guid? ImpersonatedByUserId { get; set; }
        public string? ImpersonatedByUserName { get; set; }
    }
}