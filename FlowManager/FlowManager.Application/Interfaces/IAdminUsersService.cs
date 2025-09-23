using FlowManager.Shared.DTOs;

namespace FlowManager.Application.Interfaces
{
    public interface IAdminUsersService
    {
        Task<List<UserProfileDto>> GetUsersForImpersonationAsync(string? currentUserId, string? search, int page, int pageSize);
    }
}