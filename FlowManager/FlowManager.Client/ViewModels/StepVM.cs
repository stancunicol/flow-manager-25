using FlowManager.Client.ViewModels.Team;
using FlowManager.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace FlowManager.Client.ViewModels
{
    public class StepVM
    {
        public Guid Id { get; set; }
        public Guid? FlowStepId { get; set; }
        public string? Name { get; set; } = string.Empty;

        public List<UserVM>? Users { get; set; }
        public List<TeamVM>? Teams { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
