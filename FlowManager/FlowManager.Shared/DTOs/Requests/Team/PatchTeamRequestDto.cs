
namespace FlowManager.Shared.DTOs.Requests.Team
{
    public class PatchTeamRequestDto
    {
        public string? Name { get; set; }
        public List<Guid>? UserIds { get; set; }
    }
}
