using FlowManager.Application.Interfaces;
using FlowManager.Domain.Entities;
using FlowManager.Shared.DTOs;
using FlowManager.Shared.DTOs.Requests.Impersonation;
using FlowManager.Shared.DTOs.Responses.ApiResponse;
using FlowManager.Shared.DTOs.Responses.Impersonation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace FlowManager.Server.Controllers
{
    [ApiController]
    [Route("api/admin/impersonation")]
    [Authorize(Roles = "Admin")]
    public class ImpersonationController : ControllerBase
    {
        private readonly IImpersonationService _impersonationService;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<ImpersonationController> _logger;

        public ImpersonationController(
            IImpersonationService impersonationService,
            SignInManager<User> signInManager,
            ILogger<ImpersonationController> logger)
        {
            _impersonationService = impersonationService;
            _signInManager = signInManager;
            _logger = logger;
        }

        [HttpPost("start")]
        public async Task<ActionResult<ApiResponseDto<ImpersonationResponseDto>>> StartImpersonation([FromBody] StartImpersonationRequestDto request)
        {
            try
            {
                var adminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(adminUserId))
                {
                    return BadRequest(new ApiResponseDto<ImpersonationResponseDto>
                    {
                        Success = false,
                        Message = "Unable to identify current user"
                    });
                }

                var result = await _impersonationService.StartImpersonationAsync(request, adminUserId);
                if (result == null)
                {
                    return BadRequest(new ApiResponseDto<ImpersonationResponseDto>
                    {
                        Success = false,
                        Message = "Unable to start impersonation"
                    });
                }

                await _signInManager.SignOutAsync();

                var claimsIdentity = new ClaimsIdentity(result.Claims, IdentityConstants.ApplicationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, claimsPrincipal);

                return Ok(new ApiResponseDto<ImpersonationResponseDto>
                {
                    Success = true,
                    Result = result.Response,
                    Message = "Impersonation started successfully",
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting impersonation for user {UserId}", request.UserId);
                return StatusCode(500, new ApiResponseDto<ImpersonationResponseDto>
                {
                    Success = false,
                    Message = "An error occurred while starting impersonation"
                });
            }
        }

        [HttpPost("end")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponseDto<bool>>> EndImpersonation()
        {
            try
            {
                _logger.LogInformation("EndImpersonation called. User authenticated: {IsAuthenticated}", User.Identity?.IsAuthenticated);
                _logger.LogInformation("Claims in context: {Claims}",
                    string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}")));

                if (!User.Identity?.IsAuthenticated ?? true)
                {
                    _logger.LogWarning("User not authenticated when trying to end impersonation");
                    return Unauthorized(new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }

                if (!_impersonationService.GetImpersonationStatus(User))
                {
                    return BadRequest(new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "No active impersonation session found"
                    });
                }

                var result = await _impersonationService.EndImpersonationAsync(User);
                if (result == null)
                {
                    return BadRequest(new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Original admin information not found in session"
                    });
                }

                await _signInManager.SignOutAsync();

                var adminClaimsIdentity = new ClaimsIdentity(result.AdminClaims, IdentityConstants.ApplicationScheme);
                var adminClaimsPrincipal = new ClaimsPrincipal(adminClaimsIdentity);

                await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, adminClaimsPrincipal);

                return Ok(new ApiResponseDto<bool>
                {
                    Success = true,
                    Result = true,
                    Message = "Impersonation ended successfully. Returned to admin account."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ending impersonation");
                return StatusCode(500, new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "An error occurred while ending impersonation"
                });
            }
        }

        [HttpGet("status")]
        [AllowAnonymous]
        public ActionResult<ApiResponseDto<bool>> GetImpersonationStatus()
        {
            try
            {
                var isImpersonating = _impersonationService.GetImpersonationStatus(User);
                return Ok(new ApiResponseDto<bool>
                {
                    Success = true,
                    Result = isImpersonating
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting impersonation status");
                return StatusCode(500, new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "An error occurred while checking impersonation status"
                });
            }
        }

        [HttpGet("original-admin")]
        [AllowAnonymous]
        public ActionResult<ApiResponseDto<string>> GetOriginalAdminName()
        {
            try
            {
                var originalAdminName = _impersonationService.GetOriginalAdminName(User);
                return Ok(new ApiResponseDto<string>
                {
                    Success = true,
                    Result = originalAdminName
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting original admin name");
                return StatusCode(500, new ApiResponseDto<string>
                {
                    Success = false,
                    Message = "An error occurred while getting original admin name"
                });
            }
        }

        [HttpGet("current-user")]
        [AllowAnonymous]
        public ActionResult<ApiResponseDto<string>> GetCurrentUserName()
        {
            try
            {
                var currentUserName = _impersonationService.GetCurrentUserName(User);
                return Ok(new ApiResponseDto<string>
                {
                    Success = true,
                    Result = currentUserName
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user name");
                return StatusCode(500, new ApiResponseDto<string>
                {
                    Success = false,
                    Message = "An error occurred while getting current user name"
                });
            }
        }
    }

    [ApiController]
    [Route("api/admin/users")]
    [Authorize(Roles = "Admin")]
    public class AdminUsersController : ControllerBase
    {
        private readonly IAdminUsersService _adminUsersService;
        private readonly ILogger<AdminUsersController> _logger;

        public AdminUsersController(IAdminUsersService adminUsersService, ILogger<AdminUsersController> logger)
        {
            _adminUsersService = adminUsersService;
            _logger = logger;
        }

        [HttpGet("for-impersonation")]
        public async Task<ActionResult<ApiResponseDto<List<UserProfileDto>>>> GetUsersForImpersonation(
            [FromQuery] string? search = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = await _adminUsersService.GetUsersForImpersonationAsync(currentUserId, search, page, pageSize);

                return Ok(new ApiResponseDto<List<UserProfileDto>>
                {
                    Success = true,
                    Result = result,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users for impersonation");
                return StatusCode(500, new ApiResponseDto<List<UserProfileDto>>
                {
                    Success = false,
                    Message = "An error occurred while loading users",
                    Timestamp = DateTime.UtcNow
                });
            }
        }
    }
}