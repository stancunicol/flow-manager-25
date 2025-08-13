// Application/DTOs/Requests/FormResponse/QueriedFormResponseRequestDto.cs
using FlowManager.Domain.Dtos;

namespace FlowManager.Application.DTOs.Requests.FormResponse
{
    public class QueriedFormResponseRequestDto
    {
        public Guid? FormTemplateId { get; set; }
        public Guid? StepId { get; set; }
        public Guid? UserId { get; set; }
        public string? SearchTerm { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public bool IncludeDeleted { get; set; } = false;

        // Pagination and sorting
        public bool? SortDescending { get; set; } = true;
        public string? SortBy { get; set; } = "CreatedAt";
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