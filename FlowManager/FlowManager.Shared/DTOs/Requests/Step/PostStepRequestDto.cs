using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Shared.DTOs.Requests.Step
{
    public class PostStepRequestDto
    {
        public string Name { get; set; } = string.Empty;

        public virtual List<Guid> UserIds { get; set; } = new List<Guid>();
        public virtual List<Guid> FlowIds { get; set; } = new List<Guid>();
    }
}
