using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowManager.Domain.Entities;

namespace FlowManager.Application.Interfaces
{
    public interface IUserRoleService
    {
        Task<IEnumerable<UserRole>> GetAllUserRoles();
        Task<UserRole?> GetUserRole(Guid id);
        Task<IEnumerable<UserRole>> GetRolesByUser(Guid userId);
        Task<IEnumerable<UserRole>> GetUsersByRole(RoleType roleType);
        Task<bool> UpdateUserRole(Guid id, UserRole userRole);
        Task<UserRole?> CreateUserRole(UserRole userRole);
        Task<bool> DeleteUserRole(Guid id);
    }
}
