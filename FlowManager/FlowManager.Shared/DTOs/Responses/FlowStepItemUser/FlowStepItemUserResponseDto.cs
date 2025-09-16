using FlowManager.Shared.DTOs.Responses.FlowStepItem;
using FlowManager.Shared.DTOs.Responses.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Shared.DTOs.Responses.FlowStepItemUser
{
    public class FlowStepItemUserResponseDto
    {
        public Guid? FlowStepItemId { get; set; }
        public Guid? UserId { get; set; }

        public virtual FlowStepItemResponseDto? FlowStepItem { get; set; }
        public virtual UserResponseDto? User { get; set; }
    }
}
