using FlowManager.Shared.DTOs.Requests.User;
using FlowManager.Shared.DTOs.Responses;
using FlowManager.Shared.DTOs.Responses.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Application.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserResponseDto>> GetAllUsersAsync();
        Task<IEnumerable<UserResponseDto>> GetAllModeratorsAsync();
        Task<IEnumerable<UserResponseDto>> GetAllAdminsAsync();
        Task<PagedResponseDto<UserResponseDto>> GetAllUsersFilteredAsync(QueriedUserRequestDto payload);
        Task<PagedResponseDto<UserResponseDto>> GetAllUsersQueriedAsync(QueriedUserRequestDto payload);
        Task<UserResponseDto> GetUserByIdAsync(Guid id);
        Task<UserResponseDto> AddUserAsync(PostUserRequestDto payload);
        Task<UserResponseDto> GetUserByEmailAsync(string email);
        Task<UserResponseDto> UpdateUserAsync(Guid id, PatchUserRequestDto payload);
        Task<UserResponseDto> DeleteUserAsync(Guid id);
        Task<UserResponseDto> RestoreUserAsync(Guid id);
        Task<bool> ResetPassword(Guid id, string newPassword);

        Task<List<string>> GetUserRolesByEmailAsync(string email);
        Task<bool> VerifyIfAssignedAsync(Guid id);
    }
}
