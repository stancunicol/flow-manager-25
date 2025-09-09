using FlowManager.Shared.DTOs.Responses.Role;
using FlowManager.Shared.DTOs.Responses.Step;
using FlowManager.Shared.DTOs.Responses.Team;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Shared.DTOs.Responses.User
{
    public class FlowStepUserResponseDto
    {
        public Guid FlowStepUserId { get; set; }
        public UserResponseDto User { get; set; } = new();
    }
}
