using FlowManager.Application.Interfaces;
using FlowManager.Domain.Dtos;
using FlowManager.Domain.Entities;
using FlowManager.Domain.Exceptions;
using FlowManager.Domain.IRepositories;
using FlowManager.Shared.DTOs.Requests.FormResponse;
using FlowManager.Shared.DTOs.Responses;
using FlowManager.Application.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace FlowManager.Application.Services
{
    public class FormResponseService : IFormResponseService
    {
        private readonly IFormResponseRepository _formResponseRepository;
        private readonly ILogger<FormResponseService> _logger;
        private readonly IEmailService _emailService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserService _userService;

        public FormResponseService(
            IFormResponseRepository formResponseRepository,
            ILogger<FormResponseService> logger,
            IEmailService emailService,
            IHttpContextAccessor httpContextAccessor,
            IUserService userService)
        {
            _formResponseRepository = formResponseRepository ?? throw new ArgumentNullException(nameof(formResponseRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
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
                CompletedByAdmin = fr.CompletedByAdmin,
                CompletedByAdminName = fr.CompletedByAdminName,
                ApprovedByAdmin = fr.ApprovedByAdmin,
                ApprovedByAdminName = fr.ApprovedByAdminName,
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
                CompletedByAdmin = fr.CompletedByAdmin,
                CompletedByAdminName = fr.CompletedByAdminName,
                ApprovedByAdmin = fr.ApprovedByAdmin,
                ApprovedByAdminName = fr.ApprovedByAdminName,
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
                CompletedByAdmin = formResponse.CompletedByAdmin,
                CompletedByAdminName = formResponse.CompletedByAdminName,
                ApprovedByAdmin = formResponse.ApprovedByAdmin,
                ApprovedByAdminName = formResponse.ApprovedByAdminName,
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
                CompletedByAdmin = fr.CompletedByAdmin,
                CompletedByAdminName = fr.CompletedByAdminName,
                ApprovedByAdmin = fr.ApprovedByAdmin,
                ApprovedByAdminName = fr.ApprovedByAdminName,
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

            // Check if admin is impersonating a user (form completion)
            var httpContext = _httpContextAccessor.HttpContext;
            bool isAdminCompletingForUser = false;
            string? adminName = null;
            
            if (httpContext?.User != null)
            {
                isAdminCompletingForUser = httpContext.User.FindFirst("IsImpersonating")?.Value == "true";
                adminName = httpContext.User.FindFirst("OriginalAdminName")?.Value;
            }

            var formResponse = new FormResponse
            {
                FormTemplateId = payload.FormTemplateId,
                StepId = payload.StepId,
                UserId = payload.UserId,
                ResponseFields = payload.ResponseFields,
                Status = "Pending", // Toate formularele noi încep cu Pending
                CreatedAt = DateTime.UtcNow,
                CompletedByAdmin = isAdminCompletingForUser,
                CompletedByAdminName = isAdminCompletingForUser ? adminName : null
            };

            await _formResponseRepository.AddAsync(formResponse);

            _logger.LogInformation("Form response {Id} created with status: Pending", formResponse.Id);

            // Reîncarcă pentru a avea relațiile populate
            var createdFormResponse = await _formResponseRepository.GetFormResponseByIdAsync(formResponse.Id);

            // Send email notification if admin completed form for user
            if (isAdminCompletingForUser && !string.IsNullOrEmpty(adminName))
            {
                try
                {
                    await _emailService.SendFormCompletedByAdminEmailAsync(
                        createdFormResponse.User?.Email ?? "",
                        createdFormResponse.User?.Name ?? "",
                        createdFormResponse.FormTemplate?.Name ?? "Unknown Form",
                        adminName,
                        formResponse.CreatedAt
                    );
                    _logger.LogInformation("Email sent to user {UserEmail} for form completed by admin {AdminName}", 
                        createdFormResponse.User?.Email, adminName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send email to user {UserEmail} for form completed by admin", 
                        createdFormResponse.User?.Email);
                }
            }

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
            Console.WriteLine("🔥🔥🔥 PATCH FORM RESPONSE CALLED 🔥🔥🔥");
            Console.WriteLine($"🔥 PatchFormResponseAsync called with ID: {payload.Id}");
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

            // LOGICA PENTRU REJECT
            if (!string.IsNullOrEmpty(payload.RejectReason))
            {
                formResponse.RejectReason = payload.RejectReason;
                formResponse.Status = "Rejected";
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
                var isLastStep = await _formResponseRepository.IsLastStepInFlowAsync(payload.StepId.Value);

                formResponse.StepId = payload.StepId.Value;

                // Doar dacă nu e reject, actualizează statusul
                if (string.IsNullOrEmpty(payload.RejectReason) && string.IsNullOrEmpty(formResponse.RejectReason))
                {
                    formResponse.Status = isLastStep ? "Approved" : "Pending";
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

            // Check if admin is acting (either as themselves or impersonating) for email notifications
            var httpContext = _httpContextAccessor.HttpContext;
            bool isAdminActing = false;
            string? adminName = null;
            
            if (httpContext?.User != null)
            {
                var impersonatingClaim = httpContext.User.FindFirst("IsImpersonating")?.Value;
                bool isImpersonating = impersonatingClaim == "true";
                bool isAdmin = httpContext.User.HasClaim(c => c.Type == "OriginalAdminId");
                
                isAdminActing = isAdmin; // Admin acting either as themselves or impersonating
                
                if (isImpersonating)
                {
                    // If impersonating, get the original admin name
                    adminName = httpContext.User.FindFirst("OriginalAdminName")?.Value;
                }
                else if (isAdmin)
                {
                    // If admin acting as themselves, get their name from Name claim
                    adminName = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
                }
                
                _logger.LogInformation("PatchFormResponse - IsAdmin: {IsAdmin}, IsImpersonating: '{IsImpersonating}', AdminName: '{AdminName}'", 
                    isAdmin, impersonatingClaim, adminName);
                
                // Track admin approval if admin is acting
                if (isAdminActing && !string.IsNullOrEmpty(adminName))
                {
                    formResponse.ApprovedByAdmin = true;
                    formResponse.ApprovedByAdminName = adminName;
                    _logger.LogInformation("Admin approval tracked for form {FormId} by admin {AdminName}", 
                        formResponse.Id, adminName);
                }
            }

            // Actualizează timestamp
            formResponse.UpdatedAt = DateTime.UtcNow;

            await _formResponseRepository.UpdateAsync(formResponse);

            _logger.LogInformation("Form response {Id} updated. Previous status: {PreviousStatus}, New status: {NewStatus}",
                payload.Id, previousStatus, formResponse.Status);

            // Reîncarcă entitatea cu toate relațiile pentru response
            var updatedFormResponse = await _formResponseRepository.GetFormResponseByIdAsync(payload.Id);

            // Send email notifications to all moderators when admin takes action
            _logger.LogInformation("🔥 Email Debug - IsAuthenticated: {IsAuthenticated}, IsAdmin: {IsAdmin}", 
                httpContext?.User?.Identity?.IsAuthenticated, httpContext?.User?.HasClaim(c => c.Type == "OriginalAdminId"));
                
            if (httpContext?.User?.Identity?.IsAuthenticated == true && httpContext.User.HasClaim(c => c.Type == "OriginalAdminId"))
            {
                _logger.LogInformation("🔥 Admin detected - proceeding with moderator notifications");
                try
                {

                    //var currentAdminName = httpContext.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")?.Value ?? "OriginalAdminName";
                    var currentAdminName = httpContext.User.FindFirstValue("OriginalAdminName") ?? "Admin";
                    _logger.LogInformation("🔥 Current admin name: {AdminName}", currentAdminName);
                    
                    // Get all moderators dynamically
                    var allModerators = await _userService.GetAllModeratorsAsync();
                    _logger.LogInformation("🔥 Found {ModeratorCount} moderators in system", allModerators.Count());
                    
                    foreach (var moderator in allModerators.Where(m => !string.IsNullOrEmpty(m.Email)))
                    {
                        _logger.LogInformation("🔥 Processing moderator: {ModeratorEmail}, Status: {Status}, RejectReason: '{RejectReason}', PreviousStatus: {PreviousStatus}", 
                            moderator.Email, formResponse.Status, payload.RejectReason, previousStatus);
                            
                        if (formResponse.Status == "Rejected" && !string.IsNullOrEmpty(payload.RejectReason))
                        {
                            _logger.LogInformation("🔥 SENDING REJECT EMAIL to {ModeratorEmail}", moderator.Email);
                            await _emailService.SendFormRejectedByAdminEmailAsync(
                                moderator.Email!,
                                moderator.Name ?? "Moderator",
                                updatedFormResponse.FormTemplate?.Name ?? "Form",
                                currentAdminName,
                                DateTime.UtcNow,
                                payload.RejectReason
                            );
                            
                            _logger.LogInformation("✅ Rejection email sent to moderator {ModeratorEmail} for form {FormId} rejected by admin {AdminName}", 
                                moderator.Email, updatedFormResponse.Id, currentAdminName);
                        }
                        else if (formResponse.Status == "Approved" || (formResponse.Status == "Pending" && previousStatus != "Pending"))
                        {
                            _logger.LogInformation("🔥 SENDING APPROVE EMAIL to {ModeratorEmail}", moderator.Email);
                            await _emailService.SendFormApprovedByAdminEmailAsync(
                                moderator.Email!,
                                moderator.Name ?? "Moderator",
                                updatedFormResponse.FormTemplate?.Name ?? "Form",
                                currentAdminName,
                                DateTime.UtcNow
                            );
                            
                            _logger.LogInformation("✅ Approval email sent to moderator {ModeratorEmail} for form {FormId} approved by admin {AdminName}", 
                                moderator.Email, updatedFormResponse.Id, currentAdminName);
                        }
                        else
                        {
                            _logger.LogInformation("🔥 No email sent - conditions not met for moderator {ModeratorEmail}", moderator.Email);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send email notifications to moderators");
                }
            }
            else
            {
                _logger.LogInformation("🔥 Admin check failed - no moderator notifications will be sent");
            }

            var result = new FormResponseResponseDto
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
                DeletedAt = updatedFormResponse.DeletedAt,
                CompletedByAdmin = updatedFormResponse.CompletedByAdmin,
                CompletedByAdminName = updatedFormResponse.CompletedByAdminName,
                ApprovedByAdmin = updatedFormResponse.ApprovedByAdmin,
                ApprovedByAdminName = updatedFormResponse.ApprovedByAdminName
            };

            // Add email notification info to result if admin acted
            if (isAdminActing && !string.IsNullOrEmpty(adminName))
            {
                _logger.LogInformation("🔔 ADMIN_ACTION: Admin {AdminName} performed action on form {FormId}, Status: {Status}", 
                    adminName, updatedFormResponse.Id, formResponse.Status);
            }

            return result;
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
                CompletedByAdmin = fr.CompletedByAdmin,
                CompletedByAdminName = fr.CompletedByAdminName,
                ApprovedByAdmin = fr.ApprovedByAdmin,
                ApprovedByAdminName = fr.ApprovedByAdminName,
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
                CompletedByAdmin = formResponse.CompletedByAdmin,
                CompletedByAdminName = formResponse.CompletedByAdminName,
                ApprovedByAdmin = formResponse.ApprovedByAdmin,
                ApprovedByAdminName = formResponse.ApprovedByAdminName,
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
                CompletedByAdmin = fr.CompletedByAdmin,
                CompletedByAdminName = fr.CompletedByAdminName,
                ApprovedByAdmin = fr.ApprovedByAdmin,
                ApprovedByAdminName = fr.ApprovedByAdminName,
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
                CompletedByAdmin = fr.CompletedByAdmin,
                CompletedByAdminName = fr.CompletedByAdminName,
                ApprovedByAdmin = fr.ApprovedByAdmin,
                ApprovedByAdminName = fr.ApprovedByAdminName,
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
                CompletedByAdmin = fr.CompletedByAdmin,
                CompletedByAdminName = fr.CompletedByAdminName,
                ApprovedByAdmin = fr.ApprovedByAdmin,
                ApprovedByAdminName = fr.ApprovedByAdminName,
                CreatedAt = fr.CreatedAt,
                UpdatedAt = fr.UpdatedAt,
                DeletedAt = fr.DeletedAt
            }).ToList();
        }
    }
}