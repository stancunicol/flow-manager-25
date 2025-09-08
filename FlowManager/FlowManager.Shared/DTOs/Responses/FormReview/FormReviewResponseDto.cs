using System;

namespace FlowManager.Shared.DTOs.Responses.FormReview
{
    public class FormReviewResponseDto
    {
        public Guid Id { get; set; }
        public Guid FormResponseId { get; set; }
        public string? FormTemplateName { get; set; }
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
    }
}