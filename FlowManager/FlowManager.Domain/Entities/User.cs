using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FlowManager.Domain.Entities
{
    public class User : IdentityUser<Guid>
    {   
        public string Name { get; set; } = string.Empty;
        
        // navigation properties
        public virtual ICollection<FormResponse> FormResponses { get; set; } = new List<FormResponse>();
        public virtual ICollection<UserRole> Roles { get; set; } = new List<UserRole>();

        public virtual ICollection<UserTeam> Teams { get; set; } = new List<UserTeam>();
        public virtual ICollection<StepUser> Steps { get; set; } = new List<StepUser>();
        public virtual ICollection<FlowStepUser> FlowStepUsers { get; set; } = new List<FlowStepUser>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}