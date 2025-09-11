namespace FlowManager.Client.ViewModels
{
    public class NextStepInfo
    {
        public bool HasNextStep { get; set; }
        public string? NextStepName { get; set; }
        public Guid? NextStepId { get; set; }
    }
}
