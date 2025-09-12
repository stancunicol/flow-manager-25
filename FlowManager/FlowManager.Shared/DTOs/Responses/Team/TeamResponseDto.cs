using FlowManager.Shared.DTOs.Responses.User;

namespace FlowManager.Shared.DTOs.Responses.Team
{
    public class TeamResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool IsDeleted => DeletedAt.HasValue;
        public int UsersCount { get; set; }
        public List<UserResponseDto>? Users { get; set; }
    }
}
