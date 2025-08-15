using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Application.DTOs.Requests.Flow
{
    public class QueriedFlowRequestDto
    {
        public string? Name { get; set; }
        public QueryParamsDto? QueryParams { get; set; }
    }
}
