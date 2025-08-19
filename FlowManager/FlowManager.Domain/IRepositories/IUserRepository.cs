using FlowManager.Domain.Dtos;
using FlowManager.Domain.Entities;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Domain.IRepositories
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<IEnumerable<User>> GetAllModeratorsAsync();
        Task<IEnumerable<User>> GetAllAdminsAsync();
        Task<(List<User> Data, int TotalCount)> GetAllUsersQueriedAsync(string? email, QueryParams? parameters);
        Task<(List<User> Data, int TotalCount)> GetAllUsersFilteredAsync(string? email, QueryParams? parameters);
        Task<User?> GetUserByIdAsync(Guid id, bool includeDeleted = false);
        Task<User?> AddUserAsync(User user);
        Task<User?> GetUserByEmailAsync(string email);
        Task SaveChangesAsync();

        Task<string> GetUserRoleByEmailAsync(string email);
        // ==========================================
        // METODE PENTRU TEAMS
        // ==========================================
        Task<List<User>> GetUsersByTeamIdAsync(Guid teamId);
        Task<List<User>> GetUsersWithoutTeamAsync();
        Task<User?> GetUserWithTeamAsync(Guid userId);
        Task<User?> GetUserWithTeamAndStepsAsync(Guid userId);

        // ==========================================
        // METODE PENTRU STEPS
        // ==========================================
        Task<List<User>> GetUsersByStepIdAsync(Guid stepId);

        
    }
}
