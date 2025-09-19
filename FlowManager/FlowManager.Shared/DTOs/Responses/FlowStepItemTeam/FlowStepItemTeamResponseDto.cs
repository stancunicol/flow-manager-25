using FlowManager.Shared.DTOs.Responses.FlowStepItem;
using FlowManager.Shared.DTOs.Responses.Team;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Shared.DTOs.Responses.FlowStepItemTeam
{
    public class FlowStepItemTeamResponseDto
    {
        public Guid? FlowStepItemId { get; set; }
        public Guid? TeamId { get; set; }

        public virtual FlowStepItemResponseDto? FlowStepItem { get; set; }
        public virtual TeamResponseDto? Team { get; set; }
    }
}
