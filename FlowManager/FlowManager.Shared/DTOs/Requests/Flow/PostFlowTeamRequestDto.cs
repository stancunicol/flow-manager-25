using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Shared.DTOs.Requests.Flow
{
    public class PostFlowTeamRequestDto
    {
        public Guid TeamId { get; set; }
        public List<Guid> UserIds { get; set; } = new List<Guid>();
    }
}
