using FlowManager.Shared.DTOs.Responses.User;

namespace FlowManager.Shared.DTOs.Responses.Team
{
    public class SplitUsersByTeamIdResponseDto
    {
        public Guid TeamId { get; set; }

        public List<UserResponseDto> AssignedToTeamUsers { get; set; } = new List<UserResponseDto>();
        public List<UserResponseDto> UnassignedToTeamUsers { get; set; } = new List<UserResponseDto>();
        public int TotalCountAssigned { get; set; }
        public int TotalCountUnassigned { get; set; }
        public int TotalPages { get; set; }
    }
}
