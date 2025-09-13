using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Shared.DTOs.Requests
{
    public class QueryParamsDto
    {
        public bool? SortDescending { get; set; } = false;
        public string? SortBy { get; set; } = null;
        public int? Page { get; set; }
        public int? PageSize { get; set; }
    }
}
