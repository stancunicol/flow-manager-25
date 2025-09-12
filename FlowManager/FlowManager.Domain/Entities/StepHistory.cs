
namespace FlowManager.Domain.Entities
{
    public class StepHistory
    {
        public Guid IdStepHistory { get; set; } = Guid.NewGuid();
        public Guid StepId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
        public Step Step { get; set; } = null!;
    }
}