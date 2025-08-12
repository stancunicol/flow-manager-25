using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Domain.Dtos
{
    public class QueryParams
    {
        public int? Page { get; set; } 
        public int? PageSize { get; set; } 
        public string? SortBy { get; set; } = "Id";
        public bool? SortDescending { get; set; } = false;
    }
}
