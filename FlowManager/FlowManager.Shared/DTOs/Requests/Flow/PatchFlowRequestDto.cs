using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Shared.DTOs.Requests.Flow
{
    public class PatchFlowRequestDto
    {
        public string? Name { get; set; }

        public virtual List<Guid>? StepIds { get; set; } = new List<Guid>();
    }
}
