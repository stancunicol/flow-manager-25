using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Application.DTOs
{
    public class UserProfileDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }
}
