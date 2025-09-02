using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowManager.Shared.DTOs.Responses.User;
using FlowManager.Shared.DTOs.Responses.Team;

namespace FlowManager.Shared.DTOs.Responses.FlowStep
{
    public class FlowStepResponseDto
    {
        public Guid Id { get; set; }
        public List<UserResponseDto> Users = new();
        public List<TeamResponseDto> Teams = new();
        public Guid? FlowId { get; set; }
        public Guid? StepId { get; set; }
        public string? StepName { get; set; }
        public bool? IsApproved { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
