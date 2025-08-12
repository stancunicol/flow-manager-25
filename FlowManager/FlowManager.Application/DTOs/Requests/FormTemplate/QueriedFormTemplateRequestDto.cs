using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Application.DTOs.Requests.FormTemplate
{
    public class QueriedFormTemplateRequestDto
    {
        public string? Name { get; set; }

        public QueryParamsDto QueryParams { get; set; }
    }
}
