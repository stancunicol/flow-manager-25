using FlowManager.Domain.Entities;

namespace FlowManager.Client.ViewModels
{
    public class FlowStepVM
    {
        public Guid? Id { get; set; }

        public bool? IsApproved { get; set; }
        public int? Order { get; set; }

        public Flow? Flow { get; set; } = null!;
        public Guid? FlowId { get; set; }

        public List<FlowStepItemVM> FlowStepItems { get; set; } = new List<FlowStepItemVM>();
    }
}
