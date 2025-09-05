using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Shared.DTOs.Responses.Impersonation
{
    public class ImpersonationResponseDto
    {
        public Guid SessionId { get; set; }
        public UserProfileDto ImpersonatedUser { get; set; } = new();
        public List<string> ImpersonatedUserRoles { get; set; } = new();
        public DateTime StartTime { get; set; }
    }
}
