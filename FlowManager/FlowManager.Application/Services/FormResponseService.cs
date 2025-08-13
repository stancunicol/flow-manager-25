

// Application/Services/FormResponseService.cs
using FlowManager.Application.DTOs.Requests.FormResponse;
using FlowManager.Application.DTOs.Responses;
using FlowManager.Application.Interfaces;
using FlowManager.Domain.Entities;
using FlowManager.Domain.IRepositories;
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
            try
            {
                _logger.LogInformation("Getting all form responses with query parameters");

                var queryParams = payload.ToQueryParams();

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
                    Page = payload.Page ?? 1,
                    PageSize = payload.PageSize ?? 10
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting form responses");
                throw;
            }
        }

        public async Task<FormResponseResponseDto?> GetFormResponseByIdAsync(Guid id)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting form response with ID: {Id}", id);
                throw;
            }
        }

        public async Task<FormResponseResponseDto> PostFormResponseAsync(PostFormResponseRequestDto payload)
        {
            try
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

                return await GetFormResponseByIdAsync(formResponse.Id)
                    ?? throw new InvalidOperationException("Failed to retrieve created form response");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating form response");
                throw;
            }
        }

        public async Task<FormResponseResponseDto?> PatchFormResponseAsync(PatchFormResponseRequestDto payload)
        {
            try
            {
                _logger.LogInformation("Updating form response with ID: {Id}", payload.Id);

                var formResponse = await _formResponseRepository.GetFormResponseByIdAsync(payload.Id);

                if (formResponse == null)
                    return null;

                // Update fields only if provided
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

                return await GetFormResponseByIdAsync(formResponse.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating form response with ID: {Id}", payload.Id);
                throw;
            }
        }

        public async Task<FormResponseResponseDto?> DeleteFormResponseAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Deleting form response with ID: {Id}", id);

                var formResponse = await _formResponseRepository.GetFormResponseByIdAsync(id);

                if (formResponse == null)
                    return null;

                var responseDto = await GetFormResponseByIdAsync(id);

                await _formResponseRepository.DeleteAsync(id);

                return responseDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting form response with ID: {Id}", id);
                throw;
            }
        }

        public async Task<List<FormResponseResponseDto>> GetFormResponsesByUserAsync(Guid userId)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting form responses for user: {UserId}", userId);
                throw;
            }
        }

        public async Task<List<FormResponseResponseDto>> GetFormResponsesByStepAsync(Guid stepId)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting form responses for step: {StepId}", stepId);
                throw;
            }
        }

        public async Task<List<FormResponseResponseDto>> GetFormResponsesByTemplateAsync(Guid formTemplateId)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting form responses for template: {FormTemplateId}", formTemplateId);
                throw;
            }
        }
    }
}