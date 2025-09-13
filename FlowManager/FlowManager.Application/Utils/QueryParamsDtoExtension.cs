using FlowManager.Domain.Dtos;
using FlowManager.Shared.DTOs.Requests;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Application.Utils
{
    public static class QueryParamsDtoExtension
    {
        public static QueryParams ToQueryParams(this QueryParamsDto parameters)
        {
            return new QueryParams
            {
                SortDescending = parameters.SortDescending,
                SortBy = parameters.SortBy,
                Page = parameters.Page,
                PageSize = parameters.PageSize
            };
        }
    }
}
