
namespace FlowManager.Shared.DTOs.Requests.User
{
    public class PostUserRequestDto
    {
        public string Username { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public List<Guid> Roles { get; set; } = new List<Guid>();
        public ICollection<Guid>? TeamsIds { get; set; }
        public Guid StepId { get; set; }
    }
}
