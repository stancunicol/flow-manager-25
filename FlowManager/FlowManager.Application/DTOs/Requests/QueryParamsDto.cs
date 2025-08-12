using FlowManager.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Application.DTOs.Requests
{
    public class QueryParamsDto
    {
        public bool? SortDescending { get; set; } = false;
        public string? SortBy { get; set; } = null;
        public int? Page { get; set; }
        public int? PageSize { get; set; }

        public QueryParams ToQueryParams()
        {
            return new QueryParams
            {
                SortDescending = SortDescending,
                SortBy = SortBy,
                Page = Page,
                PageSize = PageSize
            };
        }
    }
}
