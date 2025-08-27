using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Shared.DTOs.Requests.FlowStep
{
    public class PostFlowStepRequestDto
    {
        public Guid StepId { get; set; }
        public List<Guid>? UserIds { get; set; }    
        public List<Guid>? TeamIds { get; set; }
    }
}
