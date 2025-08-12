using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Application.DTOs.Requests.User
{
    public class QueriedUserRequestDto
    {
        public string? Email { get; set; }

        public QueryParamsDto? QueryParams { get; set; }
    }
}
