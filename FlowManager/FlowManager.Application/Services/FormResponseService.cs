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
using FlowManager.Shared.DTOs.Responses.FlowStepItem;

namespace FlowManager.Application.Services
{
    public class FormResponseService : IFormResponseService
    {
        private readonly IFormResponseRepository _formResponseRepository;
        private readonly ILogger<FormResponseService> _logger;
        private readonly IEmailService _emailService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserService _userService;
        private readonly IFormReviewRepository _formReviewRepository;
        private readonly IFlowRepository _flowRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IFormTemplateRepository _formTemplateRepository;

        public FormResponseService(
            IFormResponseRepository formResponseRepository,
            IFormReviewRepository formReviewRepository,
            IFlowRepository flowRepository,
            IRoleRepository roleRepository,
            IFormTemplateRepository formTemplateRepository,
            ILogger<FormResponseService> logger,
            IEmailService emailService,
            IHttpContextAccessor httpContextAccessor,
            IUserService userService)
        {
            _formResponseRepository = formResponseRepository ?? throw new ArgumentNullException(nameof(formResponseRepository));
            _formReviewRepository = formReviewRepository ?? throw new ArgumentNullException(nameof(formReviewRepository));
            _flowRepository = flowRepository ?? throw new ArgumentNullException(nameof(flowRepository));
            _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _formTemplateRepository = formTemplateRepository ?? throw new ArgumentNullException(nameof(formTemplateRepository));
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
                FlowStep = new Shared.DTOs.Responses.FlowStep.FlowStepResponseDto
                {
                    Id = fr.FlowStep?.Id ?? Guid.Empty,
                    FlowId = fr.FlowStep?.FlowId ?? Guid.Empty,
                    IsApproved = fr.FlowStep?.IsApproved ?? false,
                    FlowStepItems = fr.FlowStep?.FlowStepItems.Select(fsi => new FlowStepItemResponseDto
                    {
                        Id = fsi.Id,
                        StepId = fsi.StepId,
                        Step = new Shared.DTOs.Responses.Step.StepResponseDto
                        {
                            StepId = fsi.Step?.Id ?? Guid.Empty,
                            StepName = fsi.Step?.Name
                        },
                    }).ToList() ?? new List<FlowStepItemResponseDto>()
                },
                UserId = fr.UserId,
                UserName = fr.User?.Name,
                UserEmail = fr.User?.Email,
                CompletedByAdmin = fr.CompletedByAdmin,
                CompletedByAdminName = fr.CompletedByAdminName,
                CreatedAt = fr.CreatedAt,
                UpdatedAt = fr.UpdatedAt,
                DeletedAt = fr.DeletedAt,
                CompletedByOtherUser = fr.CompletedByOtherUser == null
                ? null : new Shared.DTOs.Responses.User.UserResponseDto
                {
                    Id = fr.CompletedByOtherUser.Id,
                    Name = fr.CompletedByOtherUser.Name
                },
                CompletedByOtherUserId = fr.CompletedByOtherUserId
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
                FlowStep = new Shared.DTOs.Responses.FlowStep.FlowStepResponseDto
                { 
                    Id = fr.FlowStep?.Id ?? Guid.Empty,
                    FlowId = fr.FlowStep?.FlowId ?? Guid.Empty,
                    IsApproved = fr.FlowStep?.IsApproved ?? false,
                    FlowStepItems = fr.FlowStep?.FlowStepItems.Select(fsi => new FlowStepItemResponseDto
                    {
                        Id = fsi.Id,
                        StepId = fsi.StepId,
                        Step = new Shared.DTOs.Responses.Step.StepResponseDto
                        {
                            StepId = fsi.Step?.Id ?? Guid.Empty,
                            StepName = fsi.Step?.Name
                        },
                    }).ToList() ?? new List<FlowStepItemResponseDto>()
                },
                UserId = fr.UserId,
                UserName = fr.User?.Name,
                UserEmail = fr.User?.Email,
                CompletedByAdmin = fr.CompletedByAdmin,
                CompletedByAdminName = fr.CompletedByAdminName,
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
                FlowStep = new Shared.DTOs.Responses.FlowStep.FlowStepResponseDto
                {
                    Id = formResponse.FlowStep?.Id ?? Guid.Empty,
                    FlowId = formResponse.FlowStep?.FlowId ?? Guid.Empty,
                    IsApproved = formResponse.FlowStep?.IsApproved ?? false,
                    FlowStepItems = formResponse.FlowStep?.FlowStepItems.Select(fsi => new FlowStepItemResponseDto
                    {
                        Id = fsi.Id,
                        StepId = fsi.StepId,
                        Step = new Shared.DTOs.Responses.Step.StepResponseDto
                        {
                            StepId = fsi.Step?.Id ?? Guid.Empty,
                            StepName = fsi.Step?.Name
                        },
                    }).ToList() ?? new List<FlowStepItemResponseDto>()
                },
                UserId = formResponse.UserId,
                UserName = formResponse.User?.Name,
                UserEmail = formResponse.User?.Email,
                CompletedByAdmin = formResponse.CompletedByAdmin,
                CompletedByAdminName = formResponse.CompletedByAdminName,
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
                FlowStep = new Shared.DTOs.Responses.FlowStep.FlowStepResponseDto
                {
                    Id = fr.FlowStep?.Id ?? Guid.Empty,
                    FlowId = fr.FlowStep?.FlowId ?? Guid.Empty,
                    IsApproved = fr.FlowStep?.IsApproved ?? false,
                    FlowStepItems = fr.FlowStep?.FlowStepItems.Select(fsi => new FlowStepItemResponseDto
                    {
                        Id = fsi.Id,
                        StepId = fsi.StepId,
                        Step = new Shared.DTOs.Responses.Step.StepResponseDto
                        {
                            StepId = fsi.Step?.Id ?? Guid.Empty,
                            StepName = fsi.Step.Name
                        },
                    }).ToList() ?? new List<FlowStepItemResponseDto>()
                },
                UserId = fr.UserId,
                UserName = fr.User?.Name,
                UserEmail = fr.User?.Email,
                CompletedByAdmin = fr.CompletedByAdmin,
                CompletedByAdminName = fr.CompletedByAdminName,
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
                UserId = payload.UserId,
                ResponseFields = payload.ResponseFields,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                CompletedByAdmin = isAdminCompletingForUser,
                CompletedByAdminName = isAdminCompletingForUser ? adminName : null
            };

            // starting a flow from first flowStep
            formResponse.FlowStep = (await _flowRepository.GetFlowByIdAsync((Guid)(await _formTemplateRepository.GetFormTemplateByIdAsync(payload.FormTemplateId))!.ActiveFlowId!))!.FlowSteps.OrderBy(fs => fs.Order).First();

            if (payload.CompletedByOtherUserId != null && payload.CompletedByOtherUserId != Guid.Empty)
            {
                formResponse.CompletedByOtherUserId = payload.CompletedByOtherUserId;
            }

            await _formResponseRepository.AddAsync(formResponse);

            _logger.LogInformation("Form response {Id} created with status: Pending", formResponse.Id);

            var createdFormResponse = await _formResponseRepository.GetFormResponseByIdAsync(formResponse.Id);

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
                FlowStep = new Shared.DTOs.Responses.FlowStep.FlowStepResponseDto
                {
                    Id = createdFormResponse.FlowStep?.Id ?? Guid.Empty,
                    FlowId = createdFormResponse.FlowStep?.FlowId ?? Guid.Empty,
                    IsApproved = createdFormResponse.FlowStep?.IsApproved ?? false,
                    FlowStepItems = createdFormResponse.FlowStep?.FlowStepItems.Select(fsi => new FlowStepItemResponseDto
                    {
                        Id = fsi.Id,
                        StepId = fsi.StepId,
                        Step = new Shared.DTOs.Responses.Step.StepResponseDto
                        {
                            StepId = fsi.Step?.Id ?? Guid.Empty,
                            StepName = fsi.Step?.Name
                        },
                    }).ToList() ?? new List<FlowStepItemResponseDto>()
                },
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
            var formResponse = await _formResponseRepository.GetFormResponseByIdAsync(payload.Id);

            if (formResponse == null)
            {
                throw new EntryNotFoundException($"Form response with id {payload.Id} was not found.");
            }

            var previousStatus = formResponse.Status;

            if (payload.ResponseFields != null)
            {
                formResponse.ResponseFields = payload.ResponseFields;
            }

            FormReview? reviewToRecord = null;

            if (!string.IsNullOrEmpty(payload.RejectReason))
            {
                formResponse.RejectReason = payload.RejectReason;
                formResponse.Status = "Rejected";
                formResponse.FlowStep.IsApproved = false;

                if (payload.ReviewerId.HasValue && payload.ReviewerStepId.HasValue)
                {
                    reviewToRecord = new FormReview
                    {
                        FormResponseId = payload.Id,
                        ReviewerId = payload.ReviewerId.Value,
                        StepId = payload.ReviewerStepId.Value,
                        Action = "Rejected",
                        RejectReason = payload.RejectReason,
                        ReviewedAt = DateTime.UtcNow
                    };
                }

                _logger.LogInformation("Form response {Id} rejected with reason: {RejectReason}", payload.Id, payload.RejectReason);
            }
            else if (payload.RejectReason == null && !string.IsNullOrEmpty(formResponse.RejectReason))
            {
                formResponse.RejectReason = null;
                formResponse.Status = "Pending";
                _logger.LogInformation("Form response {Id} reject reason cleared, status set to: {Status}", payload.Id, formResponse.Status);
            }

            Guid ModeratorRoleId = (await _roleRepository.GetRoleByRolenameAsync("MODERATOR"))!.Id;
            List<FlowStep> flowStepsForCurrentForm = (await _flowRepository.GetFlowByIdIncludeStepsAsync((Guid)formResponse.FormTemplate.ActiveFlowId, ModeratorRoleId))!.FlowSteps.OrderBy(fs => fs.Order).ToList();

            FlowStep? nextFlowStep = flowStepsForCurrentForm.FindIndex(flowStep => flowStep.Id == formResponse.FlowStepId) is int currentIndex && currentIndex >= 0 && currentIndex < flowStepsForCurrentForm.Count - 1
                ? flowStepsForCurrentForm[currentIndex + 1]
                : null;

            if (nextFlowStep != null)
            {
                if (payload.ReviewerId.HasValue && payload.ReviewerStepId.HasValue && string.IsNullOrEmpty(payload.RejectReason) && string.IsNullOrEmpty(formResponse.RejectReason))
                {
                    reviewToRecord = new FormReview
                    {
                        FormResponseId = payload.Id,
                        ReviewerId = payload.ReviewerId.Value,
                        StepId = payload.ReviewerStepId.Value,
                        Action = "Approved",
                        ReviewedAt = DateTime.UtcNow
                    };
                }

                nextFlowStep.IsApproved = true;
                formResponse.FlowStep = nextFlowStep;

                if (string.IsNullOrEmpty(payload.RejectReason) && string.IsNullOrEmpty(formResponse.RejectReason))
                {
                    formResponse.Status = "Pending";
                }
            }

            if (!string.IsNullOrEmpty(payload.Status))
            {
                formResponse.Status = payload.Status;

                if (payload.Status == "Approved" && payload.ReviewerId.HasValue && reviewToRecord == null)
                {
                    reviewToRecord = new FormReview
                    {
                        FormResponseId = payload.Id,
                        ReviewerId = payload.ReviewerId.Value,
                        StepId = payload.ReviewerStepId!.Value,
                        Action = "Approved",
                        ReviewedAt = DateTime.UtcNow
                    };
                }

                _logger.LogInformation("Form response {Id} status explicitly set to: {Status}", payload.Id, payload.Status);
            }

            var httpContext = _httpContextAccessor.HttpContext;
            bool isAdminActing = false;
            string? adminName = null;

            if (httpContext?.User != null)
            {
                var impersonatingClaim = httpContext.User.FindFirst("IsImpersonating")?.Value;
                bool isImpersonating = impersonatingClaim == "true";
                bool isAdmin = httpContext.User.HasClaim(c => c.Type == "OriginalAdminId");

                isAdminActing = isAdmin; 

                if (isImpersonating)
                {
                    adminName = httpContext.User.FindFirst("OriginalAdminName")?.Value;
                }
                else if (isAdmin)
                {
                    adminName = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
                }

                _logger.LogInformation("PatchFormResponse - IsAdmin: {IsAdmin}, IsImpersonating: '{IsImpersonating}', AdminName: '{AdminName}'",
                    isAdmin, impersonatingClaim, adminName);

                if (isAdminActing && !string.IsNullOrEmpty(adminName))
                {
                    _logger.LogInformation("Admin approval tracked for form {FormId} by admin {AdminName}",
                        formResponse.Id, adminName);
                }
            }

            formResponse.UpdatedAt = DateTime.UtcNow;

            await _formResponseRepository.UpdateAsync(formResponse);

            if (reviewToRecord != null)
            {
                if (httpContext?.User != null)
                {
                    var isImpersonating = httpContext.User.FindFirst("IsImpersonating")?.Value == "true";
                    if (isImpersonating)
                    {
                        reviewToRecord.IsImpersonatedAction = true;
                        var originalAdminIdClaim = httpContext.User.FindFirst("OriginalAdminId")?.Value;
                        if (Guid.TryParse(originalAdminIdClaim, out var originalAdminId))
                        {
                            reviewToRecord.ImpersonatedByUserId = originalAdminId;
                        }
                        reviewToRecord.ImpersonatedByUserName = httpContext.User.FindFirst("OriginalAdminName")?.Value;
                        
                        _logger.LogInformation("Review marked as impersonated action by admin {AdminName} ({AdminId})", 
                            reviewToRecord.ImpersonatedByUserName, reviewToRecord.ImpersonatedByUserId);
                    }
                }

                await _formReviewRepository.AddAsync(reviewToRecord);
                _logger.LogInformation("Review recorded: {Action} by {ReviewerId} for form {FormResponseId} at step {StepId}, IsImpersonated: {IsImpersonated}",
                    reviewToRecord.Action, reviewToRecord.ReviewerId, reviewToRecord.FormResponseId, reviewToRecord.StepId, reviewToRecord.IsImpersonatedAction);
            }

            _logger.LogInformation("Form response {Id} updated. Previous status: {PreviousStatus}, New status: {NewStatus}",
                payload.Id, previousStatus, formResponse.Status);

            var updatedFormResponse = await _formResponseRepository.GetFormResponseByIdAsync(payload.Id);

            _logger.LogInformation("🔥 Email Debug - IsAuthenticated: {IsAuthenticated}, IsAdmin: {IsAdmin}",
                httpContext?.User?.Identity?.IsAuthenticated, httpContext?.User?.HasClaim(c => c.Type == "OriginalAdminId"));

            if (httpContext?.User?.Identity?.IsAuthenticated == true && httpContext.User.HasClaim(c => c.Type == "OriginalAdminId"))
            {
                _logger.LogInformation("🔥 Admin detected - proceeding with impersonated user notification");
                try
                {
                    var currentAdminName = httpContext.User.FindFirstValue("OriginalAdminName") ?? "Admin";
                    _logger.LogInformation("🔥 Current admin name: {AdminName}", currentAdminName);

                    var impersonatedUserName = httpContext.User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name");
                    var impersonatedUserEmail = httpContext.User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress");
                    if (!string.IsNullOrEmpty(impersonatedUserName) && !string.IsNullOrEmpty(impersonatedUserEmail))
                    {
                        _logger.LogInformation("🔥 Processing impersonated user: {UserEmail}, Status: {Status}, RejectReason: '{RejectReason}', PreviousStatus: {PreviousStatus}",
                            impersonatedUserEmail, formResponse.Status, payload.RejectReason, previousStatus);

                        if (formResponse.Status == "Rejected" && !string.IsNullOrEmpty(payload.RejectReason))
                        {
                            _logger.LogInformation("🔥 SENDING REJECT EMAIL to impersonated user {UserEmail}", impersonatedUserEmail);
                            await _emailService.SendFormRejectedByAdminEmailAsync(
                                impersonatedUserEmail,
                                impersonatedUserName ?? "User",
                                updatedFormResponse.FormTemplate?.Name ?? "Form",
                                currentAdminName,
                                DateTime.UtcNow,
                                payload.RejectReason
                            );

                            _logger.LogInformation("✅ Rejection email sent to impersonated user {UserEmail} for form {FormId} rejected by admin {AdminName}",
                                impersonatedUserEmail, updatedFormResponse.Id, currentAdminName);
                        }
                        else if (formResponse.Status == "Approved" || formResponse.Status == "Pending")
                        {
                            _logger.LogInformation("🔥 SENDING APPROVE EMAIL to impersonated user {UserEmail}", impersonatedUserEmail);
                            await _emailService.SendFormApprovedByAdminEmailAsync(
                                impersonatedUserEmail,
                                impersonatedUserName ?? "User",
                                updatedFormResponse.FormTemplate?.Name ?? "Form",
                                currentAdminName,
                                DateTime.UtcNow
                            );

                            _logger.LogInformation("✅ Approval email sent to impersonated user {UserEmail} for form {FormId} approved by admin {AdminName}",
                                impersonatedUserEmail, updatedFormResponse.Id, currentAdminName);
                        }
                        else
                        {
                            _logger.LogInformation("🔥 No email sent - conditions not met for impersonated user {UserEmail}", impersonatedUserEmail);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("🔥 No impersonated user found or user has no email for form {FormId}", updatedFormResponse.Id);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send email notification to impersonated user");
                }
            }
            else
            {
                _logger.LogInformation("🔥 Admin check failed - no impersonated user notification will be sent");
            }

            var result = new FormResponseResponseDto
            {
                Id = updatedFormResponse.Id,
                RejectReason = updatedFormResponse.RejectReason,
                Status = updatedFormResponse.Status,
                ResponseFields = updatedFormResponse.ResponseFields,
                FormTemplateId = updatedFormResponse.FormTemplateId,
                FormTemplateName = updatedFormResponse.FormTemplate?.Name,
                FlowStep = new Shared.DTOs.Responses.FlowStep.FlowStepResponseDto
                {
                    Id = updatedFormResponse.FlowStep?.Id ?? Guid.Empty,
                    FlowId = updatedFormResponse.FlowStep?.FlowId ?? Guid.Empty,
                    IsApproved = updatedFormResponse.FlowStep?.IsApproved ?? false,
                    FlowStepItems = updatedFormResponse.FlowStep?.FlowStepItems.Select(fsi => new FlowStepItemResponseDto
                    {
                        Id = fsi.Id,
                        StepId = fsi.StepId,
                        Step = new Shared.DTOs.Responses.Step.StepResponseDto
                        {
                            StepId = fsi.Step?.Id ?? Guid.Empty,
                            StepName = fsi.Step?.Name
                        },
                    }).ToList() ?? new List<FlowStepItemResponseDto>()
                },
                UserId = updatedFormResponse.UserId,
                UserName = updatedFormResponse.User?.Name,
                UserEmail = updatedFormResponse.User?.Email,
                CreatedAt = updatedFormResponse.CreatedAt,
                UpdatedAt = updatedFormResponse.UpdatedAt,
                DeletedAt = updatedFormResponse.DeletedAt,
                CompletedByAdmin = updatedFormResponse.CompletedByAdmin,
                CompletedByAdminName = updatedFormResponse.CompletedByAdminName,
            };

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
                FlowStep = new Shared.DTOs.Responses.FlowStep.FlowStepResponseDto
                {
                    Id = fr.FlowStep?.Id ?? Guid.Empty,
                    FlowId = fr.FlowStep?.FlowId ?? Guid.Empty,
                    IsApproved = fr.FlowStep?.IsApproved ?? false,
                    FlowStepItems = fr.FlowStep?.FlowStepItems.Select(fsi => new FlowStepItemResponseDto
                    {
                        Id = fsi.Id,
                        StepId = fsi.StepId,
                        Step = new Shared.DTOs.Responses.Step.StepResponseDto
                        {
                            StepId = fsi.Step?.Id ?? Guid.Empty,
                            StepName = fsi.Step?.Name
                        },
                    }).ToList() ?? new List<FlowStepItemResponseDto>()
                },
                UserId = fr.UserId,
                UserName = fr.User?.Name,
                UserEmail = fr.User?.Email,
                CompletedByAdmin = fr.CompletedByAdmin,
                CompletedByAdminName = fr.CompletedByAdminName,
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
                FlowStep = new Shared.DTOs.Responses.FlowStep.FlowStepResponseDto
                {
                    Id = formResponse.FlowStep?.Id ?? Guid.Empty,
                    FlowId = formResponse.FlowStep?.FlowId ?? Guid.Empty,
                    IsApproved = formResponse.FlowStep?.IsApproved ?? false,
                    FlowStepItems = formResponse.FlowStep?.FlowStepItems.Select(fsi => new FlowStepItemResponseDto
                    {
                        Id = fsi.Id,
                        StepId = fsi.StepId,
                        Step = new Shared.DTOs.Responses.Step.StepResponseDto
                        {
                            StepId = fsi.Step?.Id ?? Guid.Empty,
                            StepName = fsi.Step?.Name
                        },
                    }).ToList() ?? new List<FlowStepItemResponseDto>()
                },
                UserId = formResponse.UserId,
                UserName = formResponse.User?.Name,
                UserEmail = formResponse.User?.Email,
                CompletedByAdmin = formResponse.CompletedByAdmin,
                CompletedByAdminName = formResponse.CompletedByAdminName,
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
                FlowStep = new Shared.DTOs.Responses.FlowStep.FlowStepResponseDto
                {
                    Id = fr.FlowStep?.Id ?? Guid.Empty,
                    FlowId = fr.FlowStep?.FlowId ?? Guid.Empty,
                    IsApproved = fr.FlowStep?.IsApproved ?? false,
                    FlowStepItems = fr.FlowStep?.FlowStepItems.Select(fsi => new FlowStepItemResponseDto
                    {
                        Id = fsi.Id,
                        StepId = fsi.StepId,
                        Step = new Shared.DTOs.Responses.Step.StepResponseDto
                        {
                            StepId = fsi.Step?.Id ?? Guid.Empty,
                            StepName = fsi.Step?.Name
                        },
                    }).ToList() ?? new List<FlowStepItemResponseDto>()
                },
                UserId = fr.UserId,
                UserName = fr.User?.Name,
                UserEmail = fr.User?.Email,
                CompletedByAdmin = fr.CompletedByAdmin,
                CompletedByAdminName = fr.CompletedByAdminName,
                CreatedAt = fr.CreatedAt,
                UpdatedAt = fr.UpdatedAt,
                DeletedAt = fr.DeletedAt
            }).ToList();
        }

        public async Task<List<FormResponseResponseDto>> GetFormResponsesByStepAsync(Guid stepId)
        {
            _logger.LogInformation("Getting form responses for step: {StepId}", stepId);

            var data = await _formResponseRepository.GetFormResponsesByFlowStepAsync(stepId);

            return data.Select(fr => new FormResponseResponseDto
            {
                Id = fr.Id,
                RejectReason = fr.RejectReason,
                ResponseFields = fr.ResponseFields,
                FormTemplateId = fr.FormTemplateId,
                FormTemplateName = fr.FormTemplate?.Name,
                FlowStep = new Shared.DTOs.Responses.FlowStep.FlowStepResponseDto
                {
                    Id = fr.FlowStep?.Id ?? Guid.Empty,
                    FlowId = fr.FlowStep?.FlowId ?? Guid.Empty,
                    IsApproved = fr.FlowStep?.IsApproved ?? false,
                    FlowStepItems = fr.FlowStep?.FlowStepItems.Select(fsi => new FlowStepItemResponseDto
                    {
                        Id = fsi.Id,
                        StepId = fsi.StepId,
                        Step = new Shared.DTOs.Responses.Step.StepResponseDto
                        {
                            StepId = fsi.Step?.Id ?? Guid.Empty,
                            StepName = fsi.Step?.Name
                        },
                    }).ToList() ?? new List<FlowStepItemResponseDto>()
                },
                UserId = fr.UserId,
                UserName = fr.User?.Name,
                UserEmail = fr.User?.Email,
                CompletedByAdmin = fr.CompletedByAdmin,
                CompletedByAdminName = fr.CompletedByAdminName,
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
                FlowStep = new Shared.DTOs.Responses.FlowStep.FlowStepResponseDto
                {
                    Id = fr.FlowStep?.Id ?? Guid.Empty,
                    FlowId = fr.FlowStep?.FlowId ?? Guid.Empty,
                    IsApproved = fr.FlowStep?.IsApproved ?? false,
                    FlowStepItems = fr.FlowStep?.FlowStepItems.Select(fsi => new FlowStepItemResponseDto
                    {
                        Id = fsi.Id,
                        StepId = fsi.StepId,
                        Step = new Shared.DTOs.Responses.Step.StepResponseDto
                        {
                            StepId = fsi.Step?.Id ?? Guid.Empty,
                            StepName = fsi.Step?.Name
                        },
                    }).ToList() ?? new List<FlowStepItemResponseDto>()
                },
                UserId = fr.UserId,
                UserName = fr.User?.Name,
                UserEmail = fr.User?.Email,
                CompletedByAdmin = fr.CompletedByAdmin,
                CompletedByAdminName = fr.CompletedByAdminName,
                CreatedAt = fr.CreatedAt,
                UpdatedAt = fr.UpdatedAt,
                DeletedAt = fr.DeletedAt
            }).ToList();
        }
    }
}