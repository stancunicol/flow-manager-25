using FlowManager.Domain.Entities;

namespace FlowManager.Domain.IRepositories
{
    public interface IRoleRepository
    {
        Task<List<Role>> GetAllRolesAsync();
        Task<Role?> GetRoleByIdAsync(Guid id);
        Task<Role?> GetRoleByRolenameAsync(string roleName);
        Task SaveChangesAsync();
    }
}
