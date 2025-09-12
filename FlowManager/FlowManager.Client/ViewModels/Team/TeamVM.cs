
namespace FlowManager.Client.ViewModels.Team
{
    public class TeamVM
    {
        public Guid Id { get; set; }

        public string? Name { get; set; } = string.Empty;

        public List<UserVM>? Users { get; set; }
        public List<StepTeamVM>? Steps { get; set; }
        public List<FlowStepTeamVM>? FlowStepTeams { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public bool? IsActive { get; set; }

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
