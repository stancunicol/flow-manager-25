
namespace FlowManager.Shared.DTOs.Responses.Team
{
    public class FlowStepTeamResponseDto
    {
        public Guid FlowStepTeamId { get; set; }
        public TeamResponseDto Team { get; set; } = new();
    }
}
