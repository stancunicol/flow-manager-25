using FlowManager.Shared.DTOs.Requests.FlowStep;
using FlowManager.Shared.DTOs.Requests.Step;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Shared.DTOs.Requests.Flow
{
    public class PostFlowRequestDto
    {
        public string Name { get; set; }

        public virtual Guid? FormTemplateId { get; set; }

        public virtual List<PostFlowStepRequestDto> Steps { get; set; } = new List<PostFlowStepRequestDto>();
    }
}
