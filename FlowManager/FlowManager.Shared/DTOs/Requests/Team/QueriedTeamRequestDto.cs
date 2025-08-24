using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Shared.DTOs.Requests.Team
{
    public class QueriedTeamRequestDto
    {
        public string? GlobalSearchTerm { get; set; }
        public string? Name { get; set; }
        public QueryParamsDto? QueryParams { get; set; }
    }
}
