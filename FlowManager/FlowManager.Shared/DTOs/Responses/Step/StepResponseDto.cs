using FlowManager.Shared.DTOs.Responses.Team;
using FlowManager.Shared.DTOs.Responses.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Shared.DTOs.Responses.Step
{
    public class StepResponseDto
    {
        public Guid StepId { get; set; }
        public string StepName { get; set; }

        public List<UserResponseDto>? Users { get;set; }
        public List<TeamResponseDto>? Teams{ get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
