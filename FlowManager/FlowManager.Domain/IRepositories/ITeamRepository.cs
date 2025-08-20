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
        Task<(List<Team>, int)> GetAllTeamsQueriedAsync(string? name, QueryParams? queryParams);
        Task<Team?> GetTeamByIdAsync(Guid id, bool includeDeleted = false);
        Task<Team?> GetTeamByNameAsync(string name);
        Task<Team?> GetTeamWithUsersAsync(Guid id);
        Task<Team> AddTeamAsync(Team team);
        Task<Team> UpdateTeamAsync(Team team);
        Task SaveChangesAsync();
    }
}
