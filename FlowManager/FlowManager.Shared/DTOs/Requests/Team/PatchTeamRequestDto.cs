using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Shared.DTOs.Requests.Team
{
    public class PatchTeamRequestDto
    {
        public string? Name { get; set; }
        public List<Guid>? UserIds { get; set; }
    }
}
