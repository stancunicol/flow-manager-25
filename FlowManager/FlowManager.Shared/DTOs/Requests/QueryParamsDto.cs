
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
