using FlowManager.Domain.Dtos;
using FlowManager.Domain.Entities;
using FlowManager.Domain.IRepositories;
using FlowManager.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace FlowManager.Infrastructure.Repositories
{
    public class TeamRepository : ITeamRepository
    {
        private readonly AppDbContext _context;

        public TeamRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Team>> GetAllTeamsAsync()
        {
            return await _context.Teams
                .Where(t => t.DeletedAt == null)
                .Include(t => t.Users.Where(u => u.DeletedAt == null))
                    .ThenInclude(ut => ut.User)
                .ToListAsync();
        }

        public async Task<(List<Team>, int)> GetAllTeamsQueriedAsync(string? globalSearchTerm, string? name, QueryParams? queryParams)
        {
            var query = _context.Teams
                .Where(t => t.DeletedAt == null)
                .Include(t => t.Users.Where(u => u.DeletedAt == null))
                    .ThenInclude(ut => ut.User)
                .AsQueryable();

            if(!string.IsNullOrWhiteSpace(globalSearchTerm))
            {
                query = query.Where(t =>
                    t.Name.Contains(globalSearchTerm) ||
                    t.Users.Any(ut => ut.User.Email!.Contains(globalSearchTerm) || ut.User.Name.Contains(globalSearchTerm)));
            }

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(t => t.Name.ToLower().Contains(name.ToLower()));
            }

            int totalCount = await query.CountAsync();

            // Sortare
            if (!string.IsNullOrEmpty(queryParams?.SortBy))
            {
                switch (queryParams.SortBy.ToLower())
                {
                    case "name":
                        query = queryParams.SortDescending == true
                            ? query.OrderByDescending(t => t.Name)
                            : query.OrderBy(t => t.Name);
                        break;
                    case "createdat":
                        query = queryParams.SortDescending == true
                            ? query.OrderByDescending(t => t.CreatedAt)
                            : query.OrderBy(t => t.CreatedAt);
                        break;
                    case "userscount":
                        query = queryParams.SortDescending == true
                            ? query.OrderByDescending(t => t.Users.Count)
                            : query.OrderBy(t => t.Users.Count);
                        break;
                    default:
                        query = query.OrderBy(t => t.Name);
                        break;
                }
            }
            else
            {
                query = query.OrderBy(t => t.Name);
            }

            // Paginare
            if (queryParams?.Page.HasValue == true && queryParams?.PageSize.HasValue == true)
            {
                int skip = (queryParams.Page.Value - 1) * queryParams.PageSize.Value;
                query = query.Skip(skip).Take(queryParams.PageSize.Value);
            }

            var teams = await query.ToListAsync();
            return (teams, totalCount);
        }

        public async Task<Team?> GetTeamByIdAsync(Guid id, bool includeDeleted = false, bool includeDeletedUserTeams = false)
        {
            var query = _context.Teams.AsQueryable();

            if (!includeDeleted)
            {
                query = query.Where(t => t.DeletedAt == null);
            }

            if (!includeDeletedUserTeams)
                query = query.Include(t => t.Users.Where(u => u.DeletedAt == null))
                    .ThenInclude(ut => ut.User);
            else
                query = query.Include(t => t.Users)
                    .ThenInclude(ut => ut.User);

            return await query.FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Team?> GetTeamByNameAsync(string name)
        {
            return await _context.Teams
                .Where(t => t.DeletedAt == null)
                .Include(t => t.Users.Where(u => u.DeletedAt == null))
                    .ThenInclude(ut => ut.User)
                .FirstOrDefaultAsync(t => t.Name.ToLower() == name.ToLower());
        }

        public async Task<Team?> GetTeamWithUsersAsync(Guid id)
        {
            return await _context.Teams
                .Where(t => t.DeletedAt == null)
                .Include(t => t.Users.Where(u => u.DeletedAt == null))
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Team> AddTeamAsync(Team team)
        {
            await _context.Teams.AddAsync(team);
            await _context.SaveChangesAsync();
            return team;
        }

        public async Task<Team> UpdateTeamAsync(Team team)
        {
            _context.Teams.Update(team);
            await _context.SaveChangesAsync();
            return team;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<(List<User>, List<User>)> GetSplitUsersByTeamIdQueriedAsync(Guid teamId, string? globalSearchTerm, QueryParams? queryParams)
        {
            List<User> assignedUsers = await _context.Teams
                .Where(t => t.Id == teamId && t.DeletedAt == null)
                .Include(t => t.Users.Where(ut => ut.DeletedAt == null))
                    .ThenInclude(ut => ut.User)
                .SelectMany(t => t.Users.Select(ut => ut.User))
                .ToListAsync();

            List<Guid> assignedUserIds = assignedUsers.Select(u => u.Id).ToList();

            List<User> unassignedUsers = await _context.Users
                .Where(u => !assignedUserIds.Contains(u.Id))
                .ToListAsync();

            if(!string.IsNullOrEmpty(globalSearchTerm))
            {
                assignedUsers = assignedUsers.Where(u => u.Name.ToUpper().Contains(globalSearchTerm.ToUpper()) ||
                                                         u.NormalizedEmail.Contains(globalSearchTerm.ToUpper()))
                                                         .ToList();

                unassignedUsers = unassignedUsers.Where(u => u.Name.ToUpper().Contains(globalSearchTerm.ToUpper()) ||
                                                         u.NormalizedEmail.Contains(globalSearchTerm.ToUpper()))
                                                         .ToList();
            }

            if(queryParams == null)
            {
                return (assignedUsers, unassignedUsers);
            }

            if (queryParams.Page == null || queryParams.Page < 0 ||
                queryParams.PageSize == null || queryParams.PageSize < 0)
            {
                return(assignedUsers, unassignedUsers);
            }
            else
            {
                assignedUsers = assignedUsers.Skip((int)queryParams.PageSize * ((int)queryParams.Page - 1))
                                                     .Take((int)queryParams.PageSize).ToList();

                unassignedUsers = unassignedUsers.Skip((int)queryParams.PageSize * ((int)queryParams.Page - 1))
                                                     .Take((int)queryParams.PageSize).ToList();

                return (assignedUsers, unassignedUsers);
            }
        }

        public async Task<(List<Team>, int)> GetAllModeratorTeamsByStepIdQueriedAsync(Guid stepId, Guid moderatorId, string? globalSearchTerm, QueryParams? queryParams)
        {
            var query = _context.Teams
                .Where(t => t.DeletedAt == null &&
                            t.Users.Any(ut => ut.DeletedAt == null &&
                                              ut.UserId == moderatorId &&
                                              ut.User.Steps.Any(su => su.StepId == stepId) &&
                                              ut.Team.Users.Any(ut => ut.DeletedAt == null && ut.UserId == moderatorId)))
                .Include(t => t.Users)
                    .ThenInclude(ut => ut.User)
                        .ThenInclude(u => u.Steps.Where(su => su.StepId == stepId))
                            .ThenInclude(us => us.Step)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(globalSearchTerm))
            {
                query = query.Where(t =>
                    t.Name.Contains(globalSearchTerm) ||
                    t.Users.Any(ut => ut.User.Email!.Contains(globalSearchTerm) || ut.User.Name.Contains(globalSearchTerm)));
            }

            int totalCount = await query.CountAsync();

            // filtering
            if (!string.IsNullOrEmpty(globalSearchTerm))
            {
                query = query.Where(t => t.Name.ToUpper() == globalSearchTerm.ToUpper() ||
                                                            t.Users.Any(ut => ut.User.NormalizedEmail == globalSearchTerm.ToUpper() ||
                                                                        ut.User.Name.Contains(globalSearchTerm)));
            }

            // pagination
            if (queryParams?.Page.HasValue == true && queryParams?.PageSize.HasValue == true)
            {
                int skip = (queryParams.Page.Value - 1) * queryParams.PageSize.Value;
                query = query.Skip(skip).Take(queryParams.PageSize.Value);
            }

            var moderatorTeams = await query.ToListAsync();
            return (moderatorTeams, totalCount);
        }
    }
}



