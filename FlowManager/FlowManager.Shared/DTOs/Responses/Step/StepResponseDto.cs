using FlowManager.Shared.DTOs.Responses.Team;
using FlowManager.Shared.DTOs.Responses.User;

namespace FlowManager.Shared.DTOs.Responses.Step
{
    public class StepResponseDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }

        public List<UserResponseDto>? Users { get;set; }
        public List<TeamResponseDto>? Teams{ get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
