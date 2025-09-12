
namespace FlowManager.Client.ViewModels
{
    public class StepUserVM
    {
        public Guid Id { get; set; }

        public StepVM Step { get; set; }

        public UserVM User { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
