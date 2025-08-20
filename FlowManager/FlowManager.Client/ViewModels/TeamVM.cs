using FlowManager.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace FlowManager.Client.ViewModels
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
    }
}
