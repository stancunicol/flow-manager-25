using FlowManager.Domain.Dtos;
using FlowManager.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Domain.IRepositories
{
    public interface ITeamRepository
    {
        Task<List<Team>> GetAllTeamsAsync();
        Task<(List<Team>, int)> GetAllTeamsQueriedAsync(string? globaleSearchTerm, string? name, QueryParams? queryParams, bool includeDeleted = false);
        Task<Team?> GetTeamByIdAsync(Guid id, bool includeDeleted = false, bool includeDeletedUserTeams = false);
        Task<Team?> GetTeamByNameAsync(string name, bool includeDeleted = false);
        Task<Team?> GetTeamWithUsersAsync(Guid id);
        Task<Team> AddTeamAsync(Team team);
        Task<Team> UpdateTeamAsync(Team team);
        Task SaveChangesAsync();
        Task<(List<User>, List<User>)> GetSplitUsersByTeamIdQueriedAsync(Guid stepId, Guid teamId, string? globalSearchTerm, QueryParams? queryParams);
        Task<(List<Team>, int)> GetAllModeratorTeamsByStepIdQueriedAsync(Guid stepId, Guid moderatorId, string? globalSearchTerm, QueryParams? queryParams);
        Task<Team?> GetTeamWithModeratorsAsync(Guid teamId, Guid moderatorId);
    }
}
