
namespace FlowManager.Shared.DTOs.Requests.User
{
    public class PatchUserRequestDto
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public string? PhoneNumber { get; set; }
        public List<Guid>? Roles { get; set; }
        public List<Guid>? TeamsIds { get; set; }
        public Guid? StepId { get; set; }
    }
}
