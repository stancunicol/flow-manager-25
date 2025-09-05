using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Shared.DTOs.Requests.Impersonation
{
    public class StartImpersonationRequestDto
    {
        public Guid UserId { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
    
}
