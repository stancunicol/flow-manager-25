using FlowManager.Shared.DTOs.Responses.Role;
using FlowManager.Shared.DTOs.Responses.Step;
using FlowManager.Shared.DTOs.Responses.Team;

namespace FlowManager.Shared.DTOs.Responses.User
{
    public class UserResponseDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public ICollection<TeamResponseDto>? Teams { get; set; }
        public List<RoleResponseDto>? Roles { get; set; }
        public StepResponseDto? Step { get; set; } 

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool? IsDeleted => DeletedAt.HasValue;
    }
}