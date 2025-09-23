using FlowManager.Domain.Entities;
using FlowManager.Shared.DTOs.Requests.Impersonation;
using FlowManager.Shared.DTOs.Responses.Impersonation;
using System.Security.Claims;

namespace FlowManager.Application.Interfaces
{
    public class ImpersonationResult
    {
        public ImpersonationResponseDto Response { get; set; } = new();
        public User AdminUser { get; set; } = new();
        public User TargetUser { get; set; } = new();
        public IList<string> TargetUserRoles { get; set; } = new List<string>();
        public List<Claim> Claims { get; set; } = new List<Claim>();
    }

    public class EndImpersonationResult
    {
        public User OriginalAdmin { get; set; } = new();
        public List<Claim> AdminClaims { get; set; } = new List<Claim>();
    }

    public interface IImpersonationService
    {
        Task<ImpersonationResult?> StartImpersonationAsync(StartImpersonationRequestDto request, string adminUserId);
        Task<EndImpersonationResult?> EndImpersonationAsync(ClaimsPrincipal currentUser);
        bool GetImpersonationStatus(ClaimsPrincipal currentUser);
        string GetOriginalAdminName(ClaimsPrincipal currentUser);
        string GetCurrentUserName(ClaimsPrincipal currentUser);
    }
}