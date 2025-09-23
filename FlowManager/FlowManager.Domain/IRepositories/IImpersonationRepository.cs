using FlowManager.Domain.Entities;

namespace FlowManager.Domain.IRepositories
{
    public interface IImpersonationRepository
    {
        Task<User?> FindUserByIdAsync(string userId);
        Task<User?> FindUserByIdWithStepAsync(Guid userId);
        Task<IList<string>> GetUserRolesAsync(User user);
    }
}