using FlowManager.Shared.DTOs.Requests.Flow;
using FlowManager.Shared.DTOs.Requests.Team;
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
        public Guid FlowId { get; set; }
        public List<Guid>? UserIds { get; set; }    
        public List<PostFlowTeamRequestDto>? Teams { get; set; }
    }
}
