namespace FlowManager.Domain.Entities
{
    public enum FormStatus
    {
        Submitted,
        Approved,
        Rejected
    }
    public class Form
    {
        public Guid Id { get; set; }
        public Guid LastStepId { get; set; }
        public FormStatus Status { get; set; } = FormStatus.Submitted;
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public Guid FlowId { get; set; }
        public Guid UserId { get; set; }
    }
}
