using FlowManager.Application.DTOs;
using FlowManager.Application.DTOs.Requests;
using FlowManager.Application.DTOs.Requests.User;
using FlowManager.Application.DTOs.Responses;
using FlowManager.Application.DTOs.Responses.User;
using FlowManager.Domain.Entities;
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
    }
}
