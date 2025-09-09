using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Shared.DTOs.Requests.Step
{
    public class PatchStepRequestDto
    {
        public string? Name { get; set; } = string.Empty;
        public List<Guid>? UserIds { get; set; }
        public List<Guid>? TeamIds { get; set; }
    }
}
