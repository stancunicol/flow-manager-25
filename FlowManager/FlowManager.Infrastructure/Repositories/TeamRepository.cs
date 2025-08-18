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
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        public async Task<(List<Team>, int)> GetAllTeamsQueriedAsync(string? name, QueryParams? queryParams)
        {
            var query = _context.Teams
                .Where(t => t.DeletedAt == null)
                .Include(t => t.Users.Where(u => u.DeletedAt == null))
                .AsQueryable();

            // Filtrare după nume dacă este specificat
            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(t => t.Name.ToLower().Contains(name.ToLower()));
            }

            // Obține totalul înaintea paginării
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

        public async Task<Team?> GetTeamByIdAsync(Guid id, bool includeDeleted = false)
        {
            var query = _context.Teams
                .Include(t => t.Users.Where(u => includeDeleted || u.DeletedAt == null))
                .AsQueryable();

            if (!includeDeleted)
            {
                query = query.Where(t => t.DeletedAt == null);
            }

            return await query.FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Team?> GetTeamByNameAsync(string name)
        {
            return await _context.Teams
                .Where(t => t.DeletedAt == null)
                .Include(t => t.Users.Where(u => u.DeletedAt == null))
                .FirstOrDefaultAsync(t => t.Name.ToLower() == name.ToLower());
        }

        public async Task<Team?> GetTeamWithUsersAsync(Guid id)
        {
            return await _context.Teams
                .Where(t => t.DeletedAt == null)
                .Include(t => t.Users.Where(u => u.DeletedAt == null))
                    .ThenInclude(u => u.Roles)
                        .ThenInclude(ur => ur.Role)
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

               
    }
}



