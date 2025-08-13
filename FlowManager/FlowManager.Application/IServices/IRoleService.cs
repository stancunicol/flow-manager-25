using FlowManager.Application.DTOs.Responses.Role;
using FlowManager.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Application.Interfaces
{
    public interface IRoleService
    {
        Task<List<RoleResponseDto>?> GetAllRolesAsync();
        Task<RoleResponseDto?> GetRoleByIdAsync(Guid id);
    }
}
