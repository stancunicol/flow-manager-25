namespace FlowManager.Client.ViewModels
{
    public class UserVM
    {
        public Guid Id { get; set; }
        public string? Name { get; set; } 
        public string? Email { get; set; }
        public bool? IsActive { get; set; }
        public List<RoleVM>? Roles { get; set; } = new List<RoleVM>();

        public override bool Equals(object obj)
        {
            if (obj is UserVM other)
                return Id == other.Id;
            return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
