
namespace FlowManager.Shared.DTOs.Requests.User
{
    public class QueriedUserRequestDto
    {
        public string? GlobalSearchTerm { get; set; }
        public string? Email { get; set; }

        public QueryParamsDto? QueryParams { get; set; }
    }
}
