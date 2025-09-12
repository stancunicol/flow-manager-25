
namespace FlowManager.Shared.DTOs.Responses.User
{
    public class FlowStepUserResponseDto
    {
        public Guid FlowStepUserId { get; set; }
        public UserResponseDto User { get; set; } = new();
    }
}
