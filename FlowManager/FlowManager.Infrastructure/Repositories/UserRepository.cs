using FlowManager.Domain.Dtos;
using FlowManager.Domain.Entities;
using FlowManager.Domain.IRepositories;
using FlowManager.Infrastructure.Context;
using FlowManager.Infrastructure.Utils;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        private IQueryable<User> GetUsersWithIncludes(IQueryable<User> query)
        {
            return query
                .Include(u => u.Roles.Where(ur => ur.DeletedAt == null))
                    .ThenInclude(ur => ur.Role)
                .Include(u => u.Team)
                .Include(u => u.Steps.Where(su => su.DeletedAt == null))
                    .ThenInclude(su => su.Step);
        }

        public async Task<User> AddUserAsync(User user)
        {
            _context.Users.Add(user);
            await SaveChangesAsync();
            return user;
        }

        public async Task<IEnumerable<User>> GetAllAdminsAsync()
        {
            var query = _context.Users
                .Where(u => u.DeletedAt == null && u.Roles.Any(r => r.Role.Name == "Admin" && r.DeletedAt == null));

            return await GetUsersWithIncludes(query).ToListAsync();
        }

        public async Task<IEnumerable<User>> GetAllModeratorsAsync()
        {
            var query = _context.Users
                .Where(u => u.DeletedAt == null && u.Roles.Any(r => r.Role.Name == "Moderator" && r.DeletedAt == null));

            return await GetUsersWithIncludes(query).ToListAsync();
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            var query = _context.Users
                .Where(u => u.DeletedAt == null);

            return await GetUsersWithIncludes(query).ToListAsync();
        }

        public async Task<(List<User> Data, int TotalCount)> GetAllUsersQueriedAsync(string? email, QueryParams parameters, bool includeDeleted = false)
        {
            IQueryable<User> query = _context.Users
                .Where(u => u.DeletedAt == null)
                .Include(u => u.Roles)
                    .ThenInclude(ur => ur.Role);

            if (!includeDeleted)
                query = query.Where(u => u.DeletedAt == null);

            if (email != null)
            {
                query = query.Where(u => u.NormalizedEmail!.Contains(email.ToUpper()));
            }

            int totalCount = await query.CountAsync();

            if (parameters == null)
            {
                var data = await query.ToListAsync();
                return (data, totalCount);
            }

            // Sortare
            if (parameters.SortBy != null)
            {
                if (parameters.SortDescending is bool SortDesc)
                    query = query.ApplySorting<User>(parameters.SortBy, SortDesc);
                else
                    query = query.ApplySorting<User>(parameters.SortBy, false);
            }

            // Paginare
            if (parameters.Page == null || parameters.Page < 0 ||
                parameters.PageSize == null || parameters.PageSize < 0)
            {
                List<User> data = await query.ToListAsync();
                return (data, totalCount);
            }
            else
            {
                List<User> data = await query.Skip((int)parameters.PageSize * ((int)parameters.Page - 1))
                                                     .Take((int)parameters.PageSize)
                                                     .ToListAsync();
                return (data, totalCount);
            }
        }

        public async Task<(List<User> Data, int TotalCount)> GetAllUsersFilteredAsync(string? email, QueryParams parameters)
        {
            IQueryable<User> query = _context.Users
                .Where(u => u.DeletedAt == null);

            if (!string.IsNullOrEmpty(email))
            {
                query = query.Where(u => u.NormalizedEmail.Contains(email.ToUpper()));
            }

            query = GetUsersWithIncludes(query);

            int totalCount = await query.CountAsync();

            if (parameters == null)
            {
                return (await query.ToListAsync(), totalCount);
            }

            // Sortare
            if (parameters.SortBy != null)
            {
                if (parameters.SortDescending is bool SortDesc)
                    query = query.ApplySorting<User>(parameters.SortBy, SortDesc);
                else
                    query = query.ApplySorting<User>(parameters.SortBy, false);
            }

            // Paginare
            if (parameters.Page == null || parameters.Page < 0 ||
                parameters.PageSize == null || parameters.PageSize < 0)
            {
                List<User> data = await query.ToListAsync();
                return (data, totalCount);
            }
            else
            {
                List<User> data = await query.Skip((int)parameters.PageSize * ((int)parameters.Page - 1))
                                                     .Take((int)parameters.PageSize)
                                                     .ToListAsync();
                return (data, totalCount);
            }
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            var query = _context.Users;

            return await query.FirstOrDefaultAsync(u => u.NormalizedEmail == email.ToUpper());
        }

        public async Task<User?> GetUserByEmailIncludeRelationshipsAsync(string email)
        {
            IQueryable<User> query = _context.Users
                .Where(u => u.DeletedAt == null && u.NormalizedEmail == email.ToUpper());

            return await GetUsersWithIncludes(query).FirstOrDefaultAsync();
        }

        public async Task<User?> GetUserByIdAsync(Guid id, bool includeDeleted = false)
        {
            IQueryable<User> query = _context.Users
                .Include(u => u.Roles)
                    .ThenInclude(ur => ur.Role);

            if (!includeDeleted)
                query = query.Where(u => u.DeletedAt == null);

            query = GetUsersWithIncludes(query);

            return await query.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<List<User>> GetUsersByTeamIdAsync(Guid teamId)
        {
            var query = _context.Users
                .Where(u => u.DeletedAt == null && u.TeamId == teamId);

            return await GetUsersWithIncludes(query)
                .OrderBy(u => u.Name)
                .ToListAsync();
        }

        public async Task<List<User>> GetUsersWithoutTeamAsync()
        {
            var query = _context.Users
                .Where(u => u.DeletedAt == null && u.TeamId == null);

            return await GetUsersWithIncludes(query)
                .OrderBy(u => u.Name)
                .ToListAsync();
        }

        public async Task<User?> GetUserWithTeamAsync(Guid userId)
        {
            var query = _context.Users
                .Where(u => u.DeletedAt == null && u.Id == userId);

            return await GetUsersWithIncludes(query).FirstOrDefaultAsync();
        }

        public async Task<User?> GetUserWithTeamAndStepsAsync(Guid userId)
        {
            return await _context.Users
                .Where(u => u.DeletedAt == null && u.Id == userId)
                .Include(u => u.Team)
                .Include(u => u.Steps.Where(su => su.DeletedAt == null))
                    .ThenInclude(su => su.Step)
                .Include(u => u.Roles.Where(ur => ur.DeletedAt == null))
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync();
        }

        public async Task<List<User>> GetUsersByStepIdAsync(Guid stepId)
        {
            return await _context.StepUsers
                .Where(su => su.DeletedAt == null && su.StepId == stepId)
                .Include(su => su.User)
                    .ThenInclude(u => u.Team)
                .Include(su => su.User.Roles.Where(ur => ur.DeletedAt == null))
                    .ThenInclude(ur => ur.Role)
                .Select(su => su.User)
                .OrderBy(u => u.Name)
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}