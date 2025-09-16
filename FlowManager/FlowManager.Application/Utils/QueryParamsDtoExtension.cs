using FlowManager.Domain.Dtos;
using FlowManager.Shared.DTOs.Requests;

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
