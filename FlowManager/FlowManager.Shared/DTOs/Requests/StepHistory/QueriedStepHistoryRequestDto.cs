namespace FlowManager.Shared.DTOs.Requests.StepHistory
{
    public class QueriedStepHistoryRequestDto
    {
        public Guid? StepId { get; set; } = Guid.Empty;
        public string? Action { get; set; } = string.Empty;

        public QueryParamsDto? QueryParams { get; set; } = new QueryParamsDto();
    }
}