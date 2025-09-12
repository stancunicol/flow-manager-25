
namespace FlowManager.Shared.DTOs.Requests.Team
{
    public class PostTeamRequestDto
    {
        public string Name { get; set; } = string.Empty;
        public List<Guid>? UserIds { get; set; } = new List<Guid>();
    }
}
