using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowManager.Shared.DTOs.Responses.User;
using FlowManager.Shared.DTOs.Responses.Team;
using FlowManager.Shared.DTOs.Responses.FlowStepItem;

namespace FlowManager.Shared.DTOs.Responses.FlowStep
{
    public class FlowStepResponseDto
    {
        public Guid Id { get; set; }
        public Guid? FlowId { get; set; }
        public List<FlowStepItemResponseDto> FlowStepItems { get; set; } = new List<FlowStepItemResponseDto>();
        public bool? IsApproved { get; set; }
        public int? Order { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
