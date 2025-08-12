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

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
