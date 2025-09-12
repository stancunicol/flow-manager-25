
namespace FlowManager.Client.ViewModels
{
    public class FlowVM
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }

        public FormTemplateVM? FormTemplate { get; set; }

        public List<FlowStepVM>? Steps { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
