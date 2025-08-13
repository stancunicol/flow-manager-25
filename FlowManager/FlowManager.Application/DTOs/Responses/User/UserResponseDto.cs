namespace FlowManager.Application.DTOs.Responses.User
{
    public class UserResponseDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool? IsDeleted => DeletedAt.HasValue;
        public List<string>? Roles { get; set; }
    }
}