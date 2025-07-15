namespace FlowManager.Domain.Entities
{
    public enum UserRole
    {
        Basic,
        Moderator,
        Admin
    }
    public class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public List<UserRole> Roles { get; set; } = new List<UserRole> { UserRole.Basic };
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
