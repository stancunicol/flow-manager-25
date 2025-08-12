using FlowManager.Application.DTOs;
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
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<IEnumerable<UserDto>> GetAllModeratorsAsync();
        Task<IEnumerable<UserDto>> GetAllAdminsAsync();
        Task<IEnumerable<UserDto>> GetAllUsersFilteredAsync(string? searchTerm = null, string? role = null);
        Task<UserDto?> GetUserByIdAsync(Guid id);
        Task<UserDto?> AddUserAsync(CreateUserDto createUserDto);
        Task<bool> UpdateUserAsync(Guid id, UpdateUserDto updateUserDto);
        Task<bool> DeleteUserAsync(Guid id);
        Task<bool> RestoreUserAsync(Guid id);
    }
}
