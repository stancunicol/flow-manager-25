namespace FlowManager.Client.ViewModels
{
    public class UserVM
    {
        public Guid Id { get; set; }
        public string? Name { get; set; } 
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }    
        public bool? IsActive { get; set; }
        public List<RoleVM>? Roles { get; set; } = new List<RoleVM>();
        public StepVM? Step { get; set; }
        public DateTime? DeletedAt { get; set; }

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
