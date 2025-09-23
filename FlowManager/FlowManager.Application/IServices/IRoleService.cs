using FlowManager.Shared.DTOs.Responses.Role;

namespace FlowManager.Application.Interfaces
{
    public interface IRoleService
    {
        Task<List<RoleResponseDto>> GetAllRolesAsync();
        Task<RoleResponseDto> GetRoleByIdAsync(Guid id);
    }
}
