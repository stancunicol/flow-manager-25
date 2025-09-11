namespace FlowManager.Shared.DTOs.Responses.StepHistory
{
    public class StepHistoryResponseDto
    {
        public Guid Id { get; set; }
        public Guid? StepId { get; set; }
        public string? Action { get; set; }
        public string? Details { get; set; }
        public DateTime DateTime { get; set; }
        public string? StepName { get; set; }
        public Guid? UserId { get; set; }
        public string? UserName { get; set; }
        public bool? IsApproved { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}