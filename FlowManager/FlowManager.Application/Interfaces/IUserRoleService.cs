using FlowManager.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Application.Interfaces
{
    public interface IUserRoleService
    {
        Task<IList<string>> GetRolesByUser(Guid userId);
        Task<IEnumerable<User>> GetUsersByRole(string roleName);
        Task<bool> AddUserToRole(Guid userId, string roleName);
        Task<bool> RemoveUserFromRole(Guid userId, string roleName);
        Task<bool> IsUserInRole(Guid userId, string roleName);
        Task<IEnumerable<IdentityRole<Guid>>> GetAllRoles();
    }
}
