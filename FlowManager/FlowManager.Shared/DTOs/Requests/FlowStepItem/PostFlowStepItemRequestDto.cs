using FlowManager.Shared.DTOs.Requests.Flow;
using FlowManager.Shared.DTOs.Requests.FlowStepItemUser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Shared.DTOs.Requests.FlowStepItem
{
    public class PostFlowStepItemRequestDto
    {
        public Guid FlowStepId { get; set; }
        public Guid StepId { get; set; }

        public virtual List<Guid> AssignedUsersIds { get; set; } = new List<Guid>();
        public virtual List<PostFlowTeamRequestDto> AssignedTeams { get; set; } = new List<PostFlowTeamRequestDto>();
    }
}
