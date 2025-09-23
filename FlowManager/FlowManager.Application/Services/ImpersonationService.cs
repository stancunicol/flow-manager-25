using FlowManager.Application.Interfaces;
using FlowManager.Domain.Entities;
using FlowManager.Domain.IRepositories;
using FlowManager.Shared.DTOs;
using FlowManager.Shared.DTOs.Requests.Impersonation;
using FlowManager.Shared.DTOs.Responses.Impersonation;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace FlowManager.Application.Services
{
    public class ImpersonationService : IImpersonationService
    {
        private readonly IImpersonationRepository _impersonationRepository;
        private readonly ILogger<ImpersonationService> _logger;

        public ImpersonationService(
            IImpersonationRepository impersonationRepository,
            ILogger<ImpersonationService> logger)
        {
            _impersonationRepository = impersonationRepository;
            _logger = logger;
        }

        public async Task<ImpersonationResult?> StartImpersonationAsync(StartImpersonationRequestDto request, string adminUserId)
        {
            var adminUser = await _impersonationRepository.FindUserByIdAsync(adminUserId);
            if (adminUser == null)
            {
                return null;
            }

            var targetUser = await _impersonationRepository.FindUserByIdWithStepAsync(request.UserId);
            if (targetUser == null)
            {
                return null;
            }

            var targetUserRoles = await _impersonationRepository.GetUserRolesAsync(targetUser);
            if (targetUserRoles.Contains("Admin"))
            {
                return null;
            }

            var sessionId = Guid.NewGuid();

            _logger.LogInformation("Admin {AdminId} ({AdminName}) started impersonating user {UserId} ({UserName}). Reason: {Reason}",
                adminUser.Id, adminUser.Name, targetUser.Id, targetUser.Name, request.Reason ?? "No reason provided");

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

            return new ImpersonationResult
            {
                Response = response,
                AdminUser = adminUser,
                TargetUser = targetUser,
                TargetUserRoles = targetUserRoles,
                Claims = claims
            };
        }

        public async Task<EndImpersonationResult?> EndImpersonationAsync(ClaimsPrincipal currentUser)
        {
            var isImpersonating = currentUser.FindFirstValue("IsImpersonating") == "true";
            if (!isImpersonating)
            {
                return null;
            }

            var originalAdminId = currentUser.FindFirstValue("OriginalAdminId");
            var originalAdminName = currentUser.FindFirstValue("OriginalAdminName");
            var impersonatedUserId = currentUser.FindFirstValue(ClaimTypes.NameIdentifier);
            var impersonatedUserName = currentUser.FindFirstValue(ClaimTypes.Name);

            if (string.IsNullOrEmpty(originalAdminId))
            {
                return null;
            }

            var originalAdmin = await _impersonationRepository.FindUserByIdAsync(originalAdminId);
            if (originalAdmin == null)
            {
                return null;
            }

            _logger.LogInformation("Admin {AdminId} ({AdminName}) ended impersonation of user {UserId} ({UserName})",
                originalAdminId, originalAdminName ?? "Unknown", impersonatedUserId ?? "Unknown", impersonatedUserName ?? "Unknown");

            var adminRoles = await _impersonationRepository.GetUserRolesAsync(originalAdmin);
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

            return new EndImpersonationResult
            {
                OriginalAdmin = originalAdmin,
                AdminClaims = adminClaims
            };
        }

        public bool GetImpersonationStatus(ClaimsPrincipal currentUser)
        {
            return currentUser.FindFirstValue("IsImpersonating") == "true";
        }

        public string GetOriginalAdminName(ClaimsPrincipal currentUser)
        {
            return currentUser.FindFirstValue("OriginalAdminName") ?? "";
        }

        public string GetCurrentUserName(ClaimsPrincipal currentUser)
        {
            return currentUser.FindFirstValue(ClaimTypes.Name) ?? "";
        }
    }
}