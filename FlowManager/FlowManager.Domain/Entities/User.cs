using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FlowManager.Domain.Entities
{
    public class User : IdentityUser<Guid>
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        public virtual ICollection<Form> Forms { get; set; } = new List<Form>();
        public virtual ICollection<StepUser> StepUsers { get; set; } = new List<StepUser>();
        public virtual ICollection<StepUpdateHistory> UpdateHistories { get; set; } = new List<StepUpdateHistory>();
    }
}
