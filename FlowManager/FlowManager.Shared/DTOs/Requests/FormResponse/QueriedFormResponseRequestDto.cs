namespace FlowManager.Shared.DTOs.Requests.FormResponse
{
    public class QueriedFormResponseRequestDto
    {
        public Guid? FormTemplateId { get; set; }
        public Guid? StepId { get; set; }
        public Guid? UserId { get; set; }
        public string? SearchTerm { get; set; }
        public List<string>? StatusFilters { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public bool IncludeDeleted { get; set; } = false;

        public QueryParamsDto? QueryParams { get; set; }
    }
}