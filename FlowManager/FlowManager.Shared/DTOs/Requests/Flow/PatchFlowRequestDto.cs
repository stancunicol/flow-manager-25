
namespace FlowManager.Shared.DTOs.Requests.Flow
{
    public class PatchFlowRequestDto
    {
        public string? Name { get; set; }

        public virtual List<Guid>? StepIds { get; set; } = new List<Guid>();
    }
}
