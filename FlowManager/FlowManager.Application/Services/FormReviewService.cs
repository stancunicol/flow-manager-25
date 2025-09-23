using FlowManager.Application.Interfaces;
using FlowManager.Application.Utils;
using FlowManager.Domain.IRepositories;
using FlowManager.Shared.DTOs.Requests.FormReview;
using FlowManager.Shared.DTOs.Responses;
using FlowManager.Shared.DTOs.Responses.FormReview;
using Microsoft.Extensions.Logging;

namespace FlowManager.Application.Services
{
    public class FormReviewService : IFormReviewService
    {
        private readonly IFormReviewRepository _formReviewRepository;
        private readonly ILogger<FormReviewService> _logger;

        public FormReviewService(
            IFormReviewRepository formReviewRepository,
            ILogger<FormReviewService> logger)
        {
            _formReviewRepository = formReviewRepository;
            _logger = logger;
        }

        public async Task<PagedResponseDto<FormReviewResponseDto>> GetReviewHistoryByModeratorAsync(Guid moderatorId, QueriedFormReviewRequestDto payload)
        {
            _logger.LogInformation("Getting review history for moderator: {ModeratorId}", moderatorId);

            var queryParams = payload.QueryParams?.ToQueryParams();

            var (data, totalCount) = await _formReviewRepository.GetReviewHistoryByModeratorPagedAsync(
                moderatorId,
                payload.SearchTerm,
                queryParams);

            var items = data
                .Where(fr => string.IsNullOrEmpty(payload.Action) || fr.Action.Equals(payload.Action, StringComparison.OrdinalIgnoreCase))
                .Select(fr => new FormReviewResponseDto
                {
                    Id = fr.Id,
                    FormResponseId = fr.FormResponseId,
                    FormTemplateId = fr.FormResponse?.FormTemplateId ?? Guid.Empty,
                    FormTemplateName = fr.FormResponse?.FormTemplate?.Name,
                    ResponseFields = fr.FormResponse?.ResponseFields ?? new Dictionary<Guid, object>(),
                    UserName = fr.FormResponse?.User?.Name,
                    UserEmail = fr.FormResponse?.User?.Email,
                    ReviewerId = fr.ReviewerId,
                    ReviewerName = fr.Reviewer?.Name,
                    StepId = fr.StepId,
                    StepName = fr.Step?.Name,
                    Action = fr.Action,
                    RejectReason = fr.RejectReason,
                    ReviewedAt = fr.ReviewedAt,
                    CreatedAt = fr.CreatedAt,
                    UpdatedAt = fr.UpdatedAt,
                    DeletedAt = fr.DeletedAt,
                    IsImpersonatedAction = fr.IsImpersonatedAction,
                    ImpersonatedByUserId = fr.ImpersonatedByUserId,
                    ImpersonatedByUserName = fr.ImpersonatedByUserName
                }).ToList();

            return new PagedResponseDto<FormReviewResponseDto>
            {
                Data = items,
                TotalCount = totalCount,
                Page = payload.QueryParams?.Page ?? 1,
                PageSize = payload.QueryParams?.PageSize ?? totalCount
            };
        }

        public async Task<List<FormReviewResponseDto>> GetReviewHistoryByFormResponseAsync(Guid formResponseId)
        {
            _logger.LogInformation("Getting review history for form response: {FormResponseId}", formResponseId);

            var data = await _formReviewRepository.GetReviewHistoryByFormResponseAsync(formResponseId);

            return data.Select(fr => new FormReviewResponseDto
            {
                Id = fr.Id,
                FormResponseId = fr.FormResponseId,
                FormTemplateId = fr.FormResponse?.FormTemplateId ?? Guid.Empty,
                ResponseFields = fr.FormResponse?.ResponseFields ?? new Dictionary<Guid, object>(),
                ReviewerId = fr.ReviewerId,
                ReviewerName = fr.Reviewer?.Name,
                StepId = fr.StepId,
                StepName = fr.Step?.Name,
                Action = fr.Action,
                RejectReason = fr.RejectReason,
                ReviewedAt = fr.ReviewedAt,
                CreatedAt = fr.CreatedAt,
                UpdatedAt = fr.UpdatedAt,
                DeletedAt = fr.DeletedAt,
                IsImpersonatedAction = fr.IsImpersonatedAction,
                ImpersonatedByUserId = fr.ImpersonatedByUserId,
                ImpersonatedByUserName = fr.ImpersonatedByUserName
            }).ToList();
        }
    }
}