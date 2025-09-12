using FlowManager.Domain.Entities;
using FlowManager.Shared.DTOs;
using FlowManager.Shared.DTOs.Requests.Impersonation;
using FlowManager.Shared.DTOs.Responses;
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
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<ImpersonationController> _logger;

        public ImpersonationController(
            UserManager<User> userManager,
            RoleManager<Role> roleManager,
            SignInManager<User> signInManager,
            ILogger<ImpersonationController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
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

                var adminUser = await _userManager.FindByIdAsync(adminUserId);
                if (adminUser == null)
                {
                    return BadRequest(new ApiResponseDto<ImpersonationResponseDto>
                    {
                        Success = false,
                        Message = "Admin user not found"
                    });
                }

                var targetUser = await _userManager.Users
                    .Include(u => u.Step)
                    .FirstOrDefaultAsync(u => u.Id == request.UserId && u.DeletedAt == null);

                if (targetUser == null)
                {
                    return NotFound(new ApiResponseDto<ImpersonationResponseDto>
                    {
                        Success = false,
                        Message = "Target user not found"
                    });
                }

                var targetUserRoles = await _userManager.GetRolesAsync(targetUser);
                if (targetUserRoles.Contains("Admin"))
                {
                    return BadRequest(new ApiResponseDto<ImpersonationResponseDto>
                    {
                        Success = false,
                        Message = "Cannot impersonate another admin user"
                    });
                }

                var sessionId = Guid.NewGuid();

                _logger.LogInformation("Admin {AdminId} ({AdminName}) started impersonating user {UserId} ({UserName}). Reason: {Reason}",
                    adminUser.Id, adminUser.Name, targetUser.Id, targetUser.Name, request.Reason ?? "No reason provided");

                await _signInManager.SignOutAsync();

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, targetUser.Id.ToString()),
                    new Claim(ClaimTypes.Name, targetUser.Name),
                    new Claim(ClaimTypes.Email, targetUser.Email ?? ""),

                    new Claim("OriginalAdminId", adminUser.Id.ToString()),
                    new Claim("OriginalAdminName", adminUser.Name),
                    new Claim("ImpersonationSessionId", sessionId.ToString()),
                    new Claim("IsImpersonating", "true")
                };

                foreach (var role in targetUserRoles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                var claimsIdentity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, claimsPrincipal);

                var response = new ImpersonationResponseDto
                {
                    SessionId = sessionId,
                    ImpersonatedUser = new UserProfileDto
                    {
                        Id = targetUser.Id,
                        Name = targetUser.Name,
                        Email = targetUser.Email ?? "",
                        Roles = targetUserRoles.ToList()
                    },
                    ImpersonatedUserRoles = targetUserRoles.ToList(),
                    StartTime = DateTime.UtcNow
                };

                return Ok(new ApiResponseDto<ImpersonationResponseDto>
                {
                    Success = true,
                    Result = response,
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

                var isImpersonating = User.FindFirstValue("IsImpersonating") == "true";
                _logger.LogInformation("IsImpersonating claim value: {IsImpersonating}", isImpersonating);
                
                if (!isImpersonating)
                {
                    return BadRequest(new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "No active impersonation session found"
                    });
                }

                var originalAdminId = User.FindFirstValue("OriginalAdminId");
                var originalAdminName = User.FindFirstValue("OriginalAdminName");
                var impersonatedUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var impersonatedUserName = User.FindFirstValue(ClaimTypes.Name);

                if (string.IsNullOrEmpty(originalAdminId))
                {
                    return BadRequest(new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Original admin information not found in session"
                    });
                }

                var originalAdmin = await _userManager.FindByIdAsync(originalAdminId);
                if (originalAdmin == null)
                {
                    return BadRequest(new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Original admin user not found"
                    });
                }

                _logger.LogInformation("Admin {AdminId} ({AdminName}) ended impersonation of user {UserId} ({UserName})",
                    originalAdminId, originalAdminName ?? "Unknown", impersonatedUserId ?? "Unknown", impersonatedUserName ?? "Unknown");

                await _signInManager.SignOutAsync();

                var adminRoles = await _userManager.GetRolesAsync(originalAdmin);
                var adminClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, originalAdmin.Id.ToString()),
                    new Claim(ClaimTypes.Name, originalAdmin.Name),
                    new Claim(ClaimTypes.Email, originalAdmin.Email ?? "")
                };

                foreach (var role in adminRoles)
                {
                    adminClaims.Add(new Claim(ClaimTypes.Role, role));
                }

                var adminClaimsIdentity = new ClaimsIdentity(adminClaims, IdentityConstants.ApplicationScheme);
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
        public async Task<ActionResult<ApiResponseDto<bool>>> GetImpersonationStatus()
        {
            try
            {
                var isImpersonating = User.FindFirstValue("IsImpersonating") == "true";
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
        public async Task<ActionResult<ApiResponseDto<string>>> GetOriginalAdminName()
        {
            try
            {
                var originalAdminName = User.FindFirstValue("OriginalAdminName");
                return Ok(new ApiResponseDto<string>
                {
                    Success = true,
                    Result = originalAdminName ?? ""
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
        public async Task<ActionResult<ApiResponseDto<string>>> GetCurrentUserName()
        {
            try
            {
                var currentUserName = User.FindFirstValue(ClaimTypes.Name);
                return Ok(new ApiResponseDto<string>
                {
                    Success = true,
                    Result = currentUserName ?? ""
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
        private readonly UserManager<User> _userManager;
        private readonly ILogger<AdminUsersController> _logger;

        public AdminUsersController(UserManager<User> userManager, ILogger<AdminUsersController> logger)
        {
            _userManager = userManager;
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
                var query = _userManager.Users
                    .Include(u => u.Step)
                    .Include(u => u.Roles)
                    .ThenInclude(ur => ur.Role)
                    .Where(u => u.DeletedAt == null);

                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(currentUserId))
                {
                    query = query.Where(u => u.Id.ToString() != currentUserId);
                }

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var searchTerm = search.Trim().ToLower();
                    query = query.Where(u =>
                        u.Name.ToLower().Contains(searchTerm) ||
                        (u.Email != null && u.Email.ToLower().Contains(searchTerm)));
                }

                var users = await query
                    .OrderBy(u => u.Name)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var result = new List<UserProfileDto>();

                foreach (var user in users)
                {
                    var roles = user.Roles.Select(ur => ur.Role.Name).ToList();

                    if (roles.Contains("Admin"))
                        continue;

                    result.Add(new UserProfileDto
                    {
                        Id = user.Id,
                        Name = user.Name,
                        Email = user.Email ?? "",
                        UserName = user.UserName,
                        Roles = roles
                    });
                }

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