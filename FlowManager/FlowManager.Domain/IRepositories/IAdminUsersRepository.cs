using FlowManager.Domain.Entities;

namespace FlowManager.Domain.IRepositories
{
    public interface IAdminUsersRepository
    {
        Task<List<User>> GetUsersForImpersonationAsync(string? currentUserId, string? search, int page, int pageSize);
    }
}