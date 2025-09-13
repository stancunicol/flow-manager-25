using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Shared.DTOs.Requests.Component
{
    public class QueriedComponentRequestDto
    {
        public string? Label { get; set; }
        public string? Type { get; set; }
        public QueryParamsDto? QueryParams { get; set; }

        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
