using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowManager.Domain.Dtos;

namespace FlowManager.Shared.DTOs.Requests.FlowStep
{
    public class QueriedFlowStepRequestDto
    {
        public string? Name { get; set; }
        public QueryParamsDto? QueryParams { get; set; }
    }
}
