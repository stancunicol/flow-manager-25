namespace FlowManager.Client.ViewModels
{
    public class UserVM
    {
        public Guid Id { get; set; }
        public string? Name { get; set; } 
        public string? Email { get; set; }
        public bool? IsActive { get; set; }
        public List<RoleVM>? Roles { get; set; } = new List<RoleVM>();
    }
}
