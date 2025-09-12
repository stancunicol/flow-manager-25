using FlowManager.Shared.DTOs.Requests.FlowStep;

namespace FlowManager.Shared.DTOs.Requests.Flow
{
    public class PostFlowRequestDto
    {
        public string Name { get; set; } = string.Empty;

        public virtual Guid? FormTemplateId { get; set; }

        public virtual List<PostFlowStepRequestDto> Steps { get; set; } = new List<PostFlowStepRequestDto>();
    }
}
