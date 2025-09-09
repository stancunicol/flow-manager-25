using FlowManager.Application.Interfaces;
using FlowManager.Domain.Dtos;
using FlowManager.Domain.Entities;
using FlowManager.Domain.Exceptions;
using FlowManager.Domain.IRepositories;
using FlowManager.Shared.DTOs.Requests.FormResponse;
using FlowManager.Shared.DTOs.Responses;
using FlowManager.Application.Utils;
using Microsoft.Extensions.Logging;

namespace FlowManager.Application.Services
{
    public class FormResponseService : IFormResponseService
    {
        private readonly IFormResponseRepository _formResponseRepository;
        private readonly ILogger<FormResponseService> _logger;
        private readonly IFormReviewRepository _formReviewRepository;

        public FormResponseService(
            IFormResponseRepository formResponseRepository,
            IFormReviewRepository formReviewRepository,
            ILogger<FormResponseService> logger)
        {
            _formResponseRepository = formResponseRepository ?? throw new ArgumentNullException(nameof(formResponseRepository));
            _formReviewRepository = formReviewRepository ?? throw new ArgumentNullException(nameof(formReviewRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<PagedResponseDto<FormResponseResponseDto>> GetAllFormResponsesQueriedAsync(QueriedFormResponseRequestDto payload)
        {
            _logger.LogInformation("Getting all form responses with query parameters");

            QueryParams? queryParams = payload.QueryParams?.ToQueryParams();

            var (data, totalCount) = await _formResponseRepository.GetAllFormResponsesQueriedAsync(
                payload.FormTemplateId,
                payload.StepId,
                payload.UserId,
                payload.SearchTerm,
                payload.CreatedFrom,
                payload.CreatedTo,
                payload.IncludeDeleted,
                queryParams,
                payload.StatusFilters);

            var items = data.Select(fr => new FormResponseResponseDto
            {
                Id = fr.Id,
                RejectReason = fr.RejectReason,
                Status = fr.Status,
                ResponseFields = fr.ResponseFields,
                FormTemplateId = fr.FormTemplateId,
                FormTemplateName = fr.FormTemplate?.Name,
                StepId = fr.StepId,
                StepName = fr.Step?.Name,
                UserId = fr.UserId,
                UserName = fr.User?.Name,
                UserEmail = fr.User?.Email,
                CreatedAt = fr.CreatedAt,
                UpdatedAt = fr.UpdatedAt,
                DeletedAt = fr.DeletedAt
            }).ToList();

            return new PagedResponseDto<FormResponseResponseDto>
            {
                Data = items,
                TotalCount = totalCount,
                Page = payload.QueryParams?.Page ?? 1,
                PageSize = payload.QueryParams?.PageSize ?? totalCount
            };
        }

        public async Task<List<FormResponseResponseDto>> GetAllFormResponsesAsync()
        {
            _logger.LogInformation("Getting all form responses without pagination");

            var data = await _formResponseRepository.GetAllFormResponsesAsync();

            return data.Select(fr => new FormResponseResponseDto
            {
                Id = fr.Id,
                RejectReason = fr.RejectReason,
                Status = fr.Status,
                ResponseFields = fr.ResponseFields,
                FormTemplateId = fr.FormTemplateId,
                FormTemplateName = fr.FormTemplate?.Name,
                StepId = fr.StepId,
                StepName = fr.Step?.Name,
                UserId = fr.UserId,
                UserName = fr.User?.Name,
                UserEmail = fr.User?.Email,
                CreatedAt = fr.CreatedAt,
                UpdatedAt = fr.UpdatedAt,
                DeletedAt = fr.DeletedAt
            }).ToList();
        }

        public async Task<FormResponseResponseDto?> GetFormResponseByIdAsync(Guid id)
        {
            _logger.LogInformation("Getting form response with ID: {Id}", id);

            var formResponse = await _formResponseRepository.GetFormResponseByIdAsync(id);

            if (formResponse == null)
                return null;

            return new FormResponseResponseDto
            {
                Id = formResponse.Id,
                RejectReason = formResponse.RejectReason,
                Status = formResponse.Status,
                ResponseFields = formResponse.ResponseFields,
                FormTemplateId = formResponse.FormTemplateId,
                FormTemplateName = formResponse.FormTemplate?.Name,
                StepId = formResponse.StepId,
                StepName = formResponse.Step?.Name,
                UserId = formResponse.UserId,
                UserName = formResponse.User?.Name,
                UserEmail = formResponse.User?.Email,
                CreatedAt = formResponse.CreatedAt,
                UpdatedAt = formResponse.UpdatedAt,
                DeletedAt = formResponse.DeletedAt
            };
        }

        public async Task<PagedResponseDto<FormResponseResponseDto>> GetFormResponsesAssignedToModeratorAsync(Guid moderatorId, QueriedFormResponseRequestDto payload)
        {
            _logger.LogInformation("Getting form responses assigned to moderator: {ModeratorId}", moderatorId);

            QueryParams? queryParams = payload.QueryParams?.ToQueryParams();

            var (data, totalCount) = await _formResponseRepository.GetFormResponsesAssignedToModeratorAsync(
                moderatorId,
                payload.SearchTerm,
                payload.CreatedFrom,
                payload.CreatedTo,
                payload.IncludeDeleted,
                queryParams);

            var items = data.Select(fr => new FormResponseResponseDto
            {
                Id = fr.Id,
                RejectReason = fr.RejectReason,
                Status = fr.Status,
                ResponseFields = fr.ResponseFields,
                FormTemplateId = fr.FormTemplateId,
                FormTemplateName = fr.FormTemplate?.Name,
                StepId = fr.StepId,
                StepName = fr.Step?.Name,
                UserId = fr.UserId,
                UserName = fr.User?.Name,
                UserEmail = fr.User?.Email,
                CreatedAt = fr.CreatedAt,
                UpdatedAt = fr.UpdatedAt,
                DeletedAt = fr.DeletedAt
            }).ToList();

            return new PagedResponseDto<FormResponseResponseDto>
            {
                Data = items,
                TotalCount = totalCount,
                Page = payload.QueryParams?.Page ?? 1,
                PageSize = payload.QueryParams?.PageSize ?? totalCount
            };
        }

        public async Task<FormResponseResponseDto> PostFormResponseAsync(PostFormResponseRequestDto payload)
        {
            _logger.LogInformation("Creating new form response");

            var formResponse = new FormResponse
            {
                FormTemplateId = payload.FormTemplateId,
                StepId = payload.StepId,
                UserId = payload.UserId,
                ResponseFields = payload.ResponseFields,
                Status = "Pending", // Toate formularele noi încep cu Pending
                CreatedAt = DateTime.UtcNow
            };

            if(payload.CompletedByOtherUserId != null && payload.CompletedByOtherUserId != Guid.Empty)
            {
                formResponse.CompletedByOtherUserId = payload.CompletedByOtherUserId;
            }

            await _formResponseRepository.AddAsync(formResponse);

            _logger.LogInformation("Form response {Id} created with status: Pending", formResponse.Id);

            // Reîncarcă pentru a avea relațiile populate
            var createdFormResponse = await _formResponseRepository.GetFormResponseByIdAsync(formResponse.Id);

            return new FormResponseResponseDto
            {
                Id = createdFormResponse.Id,
                RejectReason = createdFormResponse.RejectReason,
                Status = createdFormResponse.Status,
                ResponseFields = createdFormResponse.ResponseFields,
                FormTemplateId = createdFormResponse.FormTemplateId,
                FormTemplateName = createdFormResponse.FormTemplate?.Name,
                StepId = createdFormResponse.StepId,
                StepName = createdFormResponse.Step?.Name,
                UserId = createdFormResponse.UserId,
                UserName = createdFormResponse.User?.Name,
                UserEmail = createdFormResponse.User?.Email,
                CreatedAt = createdFormResponse.CreatedAt,
                UpdatedAt = createdFormResponse.UpdatedAt,
                DeletedAt = createdFormResponse.DeletedAt
            };
        }

        public async Task<FormResponseResponseDto> PatchFormResponseAsync(PatchFormResponseRequestDto payload)
        {
            _logger.LogInformation("Updating form response with ID: {Id}", payload.Id);

            var formResponse = await _formResponseRepository.GetFormResponseByIdAsync(payload.Id);

            if (formResponse == null)
            {
                throw new EntryNotFoundException($"Form response with id {payload.Id} was not found.");
            }

            // Salvează statusul anterior pentru logging
            var previousStatus = formResponse.Status;
            var previousStepId = formResponse.StepId;

            // Actualizează ResponseFields dacă sunt furnizate
            if (payload.ResponseFields != null)
            {
                formResponse.ResponseFields = payload.ResponseFields;
            }

            // ÎNREGISTREAZĂ REVIEW-UL PENTRU ISTORIC
            FormReview? reviewToRecord = null;

            // LOGICA PENTRU REJECT
            if (!string.IsNullOrEmpty(payload.RejectReason))
            {
                formResponse.RejectReason = payload.RejectReason;
                formResponse.Status = "Rejected";

                // Înregistrează reject-ul în istoric
                if (payload.ReviewerId.HasValue)
                {
                    reviewToRecord = new FormReview
                    {
                        FormResponseId = payload.Id,
                        ReviewerId = payload.ReviewerId.Value,
                        StepId = formResponse.StepId,
                        Action = "Rejected",
                        RejectReason = payload.RejectReason,
                        ReviewedAt = DateTime.UtcNow
                    };
                }

                _logger.LogInformation("Form response {Id} rejected with reason: {RejectReason}", payload.Id, payload.RejectReason);
            }
            // LOGICA PENTRU CLEAR REJECT (approve după reject anterior)
            else if (payload.RejectReason == null && !string.IsNullOrEmpty(formResponse.RejectReason))
            {
                formResponse.RejectReason = null;
                // Verifică dacă e ultimul step pentru a decide statusul
                if (payload.StepId.HasValue)
                {
                    var isLastStep = await _formResponseRepository.IsLastStepInFlowAsync(payload.StepId.Value);
                    formResponse.Status = isLastStep ? "Approved" : "Pending";
                }
                else
                {
                    formResponse.Status = "Pending";
                }
                _logger.LogInformation("Form response {Id} reject reason cleared, status set to: {Status}", payload.Id, formResponse.Status);
            }

            // LOGICA PENTRU SCHIMBAREA STEP-ULUI (approve și move to next step)
            if (payload.StepId.HasValue && payload.StepId.Value != formResponse.StepId)
            {
                // Înregistrează approve-ul pentru step-ul curent ÎNAINTE de mutare
                if (payload.ReviewerId.HasValue && string.IsNullOrEmpty(payload.RejectReason) && string.IsNullOrEmpty(formResponse.RejectReason))
                {
                    reviewToRecord = new FormReview
                    {
                        FormResponseId = payload.Id,
                        ReviewerId = payload.ReviewerId.Value,
                        StepId = formResponse.StepId, // Step-ul curent (nu cel nou)
                        Action = "Approved",
                        ReviewedAt = DateTime.UtcNow
                    };
                }

                formResponse.StepId = payload.StepId.Value;

                // SCHIMBAT: Nu setez automat status-ul la "Approved" când ajunge la ultimul step
                // Formularele rămân "Pending" la fiecare step pentru review de către moderatori
                if (string.IsNullOrEmpty(payload.RejectReason) && string.IsNullOrEmpty(formResponse.RejectReason))
                {
                    formResponse.Status = "Pending"; // Întotdeauna "Pending" pentru review
                }

                _logger.LogInformation("Form response {Id} moved from step {PreviousStepId} to step {NewStepId}, status: {Status}",
                    payload.Id, previousStepId, payload.StepId.Value, formResponse.Status);
            }

            // OVERRIDE EXPLICIT PENTRU STATUS (dacă e specificat explicit în payload)
            if (!string.IsNullOrEmpty(payload.Status))
            {
                formResponse.Status = payload.Status;
                _logger.LogInformation("Form response {Id} status explicitly set to: {Status}", payload.Id, payload.Status);
            }

            // Actualizează timestamp
            formResponse.UpdatedAt = DateTime.UtcNow;

            await _formResponseRepository.UpdateAsync(formResponse);

            // SALVEAZĂ REVIEW-UL ÎN ISTORIC
            if (reviewToRecord != null)
            {
                await _formReviewRepository.AddAsync(reviewToRecord);
                _logger.LogInformation("Review recorded: {Action} by {ReviewerId} for form {FormResponseId} at step {StepId}",
                    reviewToRecord.Action, reviewToRecord.ReviewerId, reviewToRecord.FormResponseId, reviewToRecord.StepId);
            }

            _logger.LogInformation("Form response {Id} updated. Previous status: {PreviousStatus}, New status: {NewStatus}",
                payload.Id, previousStatus, formResponse.Status);

            // Reîncarcă entitatea cu toate relațiile pentru response
            var updatedFormResponse = await _formResponseRepository.GetFormResponseByIdAsync(payload.Id);

            return new FormResponseResponseDto
            {
                Id = updatedFormResponse.Id,
                RejectReason = updatedFormResponse.RejectReason,
                Status = updatedFormResponse.Status,
                ResponseFields = updatedFormResponse.ResponseFields,
                FormTemplateId = updatedFormResponse.FormTemplateId,
                FormTemplateName = updatedFormResponse.FormTemplate?.Name,
                StepId = updatedFormResponse.StepId,
                StepName = updatedFormResponse.Step?.Name,
                UserId = updatedFormResponse.UserId,
                UserName = updatedFormResponse.User?.Name,
                UserEmail = updatedFormResponse.User?.Email,
                CreatedAt = updatedFormResponse.CreatedAt,
                UpdatedAt = updatedFormResponse.UpdatedAt,
                DeletedAt = updatedFormResponse.DeletedAt
            };
        }

        public async Task<List<FormResponseResponseDto>> GetFormResponsesByStatusAsync(string status)
        {
            _logger.LogInformation("Getting form responses with status: {Status}", status);

            var data = await _formResponseRepository.GetFormResponsesByStatusAsync(status);

            return data.Select(fr => new FormResponseResponseDto
            {
                Id = fr.Id,
                RejectReason = fr.RejectReason,
                Status = fr.Status,
                ResponseFields = fr.ResponseFields,
                FormTemplateId = fr.FormTemplateId,
                FormTemplateName = fr.FormTemplate?.Name,
                StepId = fr.StepId,
                StepName = fr.Step?.Name,
                UserId = fr.UserId,
                UserName = fr.User?.Name,
                UserEmail = fr.User?.Email,
                CreatedAt = fr.CreatedAt,
                UpdatedAt = fr.UpdatedAt,
                DeletedAt = fr.DeletedAt
            }).ToList();
        }

        public async Task<FormResponseResponseDto> DeleteFormResponseAsync(Guid id)
        {
            _logger.LogInformation("Deleting form response with ID: {Id}", id);

            var formResponse = await _formResponseRepository.GetFormResponseByIdAsync(id);

            if (formResponse == null)
            {
                throw new EntryNotFoundException($"Form response with id {id} not found.");
            }

            await _formResponseRepository.DeleteAsync(id);

            return new FormResponseResponseDto
            {
                Id = formResponse.Id,
                RejectReason = formResponse.RejectReason,
                ResponseFields = formResponse.ResponseFields,
                FormTemplateId = formResponse.FormTemplateId,
                FormTemplateName = formResponse.FormTemplate?.Name,
                StepId = formResponse.StepId,
                StepName = formResponse.Step?.Name,
                UserId = formResponse.UserId,
                UserName = formResponse.User?.Name,
                UserEmail = formResponse.User?.Email,
                CreatedAt = formResponse.CreatedAt,
                UpdatedAt = formResponse.UpdatedAt,
                DeletedAt = formResponse.DeletedAt
            };
        }

        public async Task<List<FormResponseResponseDto>> GetFormResponsesByUserAsync(Guid userId)
        {
            _logger.LogInformation("Getting form responses for user: {UserId}", userId);

            var data = await _formResponseRepository.GetFormResponsesByUserAsync(userId);

            return data.Select(fr => new FormResponseResponseDto
            {
                Id = fr.Id,
                RejectReason = fr.RejectReason,
                Status = fr.Status,
                ResponseFields = fr.ResponseFields,
                FormTemplateId = fr.FormTemplateId,
                FormTemplateName = fr.FormTemplate?.Name,
                StepId = fr.StepId,
                StepName = fr.Step?.Name,
                UserId = fr.UserId,
                UserName = fr.User?.Name,
                UserEmail = fr.User?.Email,
                CreatedAt = fr.CreatedAt,
                UpdatedAt = fr.UpdatedAt,
                DeletedAt = fr.DeletedAt
            }).ToList();
        }

        public async Task<List<FormResponseResponseDto>> GetFormResponsesByStepAsync(Guid stepId)
        {
            _logger.LogInformation("Getting form responses for step: {StepId}", stepId);

            var data = await _formResponseRepository.GetFormResponsesByStepAsync(stepId);

            return data.Select(fr => new FormResponseResponseDto
            {
                Id = fr.Id,
                RejectReason = fr.RejectReason,
                ResponseFields = fr.ResponseFields,
                FormTemplateId = fr.FormTemplateId,
                FormTemplateName = fr.FormTemplate?.Name,
                StepId = fr.StepId,
                StepName = fr.Step?.Name,
                UserId = fr.UserId,
                UserName = fr.User?.Name,
                UserEmail = fr.User?.Email,
                CreatedAt = fr.CreatedAt,
                UpdatedAt = fr.UpdatedAt,
                DeletedAt = fr.DeletedAt
            }).ToList();
        }

        public async Task<List<FormResponseResponseDto>> GetFormResponsesByTemplateAsync(Guid formTemplateId)
        {
            _logger.LogInformation("Getting form responses for template: {FormTemplateId}", formTemplateId);

            var data = await _formResponseRepository.GetFormResponsesByTemplateAsync(formTemplateId);

            return data.Select(fr => new FormResponseResponseDto
            {
                Id = fr.Id,
                RejectReason = fr.RejectReason,
                ResponseFields = fr.ResponseFields,
                FormTemplateId = fr.FormTemplateId,
                FormTemplateName = fr.FormTemplate?.Name,
                StepId = fr.StepId,
                StepName = fr.Step?.Name,
                UserId = fr.UserId,
                UserName = fr.User?.Name,
                UserEmail = fr.User?.Email,
                CreatedAt = fr.CreatedAt,
                UpdatedAt = fr.UpdatedAt,
                DeletedAt = fr.DeletedAt
            }).ToList();
        }
    }
}