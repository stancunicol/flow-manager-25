using FlowManager.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Application.Interfaces
{
    public interface IUserRoleService
    {
        Task<IEnumerable<UserRole>> GetAllUserRoles();
        Task<UserRole?> GetUserRole(Guid userId, Guid roleId);
        Task<IEnumerable<UserRole>> GetRolesByUser(Guid userId);
        Task<IEnumerable<UserRole>> GetUsersByRole(string roleName);
        Task<bool> UpdateUserRole(Guid userId, Guid roleId, UserRole userRole);
        Task<UserRole?> CreateUserRole(UserRole userRole);
        Task<bool> DeleteUserRole(Guid userId, Guid roleId);
    }
}
