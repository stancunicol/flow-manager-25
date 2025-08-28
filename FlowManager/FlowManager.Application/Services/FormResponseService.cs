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

        public FormResponseService(
            IFormResponseRepository formResponseRepository,
            ILogger<FormResponseService> logger)
        {
            _formResponseRepository = formResponseRepository ?? throw new ArgumentNullException(nameof(formResponseRepository));
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
                queryParams);

            var items = data.Select(fr => new FormResponseResponseDto
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
                CreatedAt = DateTime.UtcNow
            };

            await _formResponseRepository.AddAsync(formResponse);

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

        public async Task<FormResponseResponseDto> PatchFormResponseAsync(PatchFormResponseRequestDto payload)
        {
            _logger.LogInformation("Updating form response with ID: {Id}", payload.Id);

            var formResponse = await _formResponseRepository.GetFormResponseByIdAsync(payload.Id);

            if (formResponse == null)
            {
                throw new EntryNotFoundException($"Form response with id {payload.Id} was not found.");
            }

            if (payload.ResponseFields != null)
            {
                formResponse.ResponseFields = payload.ResponseFields;
            }

            if (!string.IsNullOrEmpty(payload.RejectReason))
            {
                formResponse.RejectReason = payload.RejectReason;
            }

            if (payload.StepId.HasValue)
            {
                formResponse.StepId = payload.StepId.Value;
            }

            await _formResponseRepository.UpdateAsync(formResponse);

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