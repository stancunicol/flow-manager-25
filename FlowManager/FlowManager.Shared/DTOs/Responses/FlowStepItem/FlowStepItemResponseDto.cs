using FlowManager.Shared.DTOs.Responses.FlowStep;
using FlowManager.Shared.DTOs.Responses.FlowStepItemTeam;
using FlowManager.Shared.DTOs.Responses.FlowStepItemUser;
using FlowManager.Shared.DTOs.Responses.Step;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Shared.DTOs.Responses.FlowStepItem
{
    public class FlowStepItemResponseDto
    {
        public virtual FlowStepResponseDto? FlowStep { get; set; }
        public Guid? FlowStepId { get; set; } 

        public virtual StepResponseDto? Step { get; set; }
        public Guid? StepId { get; set; }

        public virtual List<FlowStepItemUserResponseDto>? AssignedUsers { get; set; } 
        public virtual List<FlowStepItemTeamResponseDto>? AssignedTeams { get; set; }

        public int? Order { get; set; }
    }
}
