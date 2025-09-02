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
                .Include(u => u.Teams)
                    .ThenInclude(ut => ut.Team)
                .Include(u => u.Step);
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

        public async Task<(List<User> Data, int TotalCount)> GetAllUsersQueriedAsync(string? email,string? globalSearchTerm, QueryParams? parameters, bool includeDeleted = false)
        {
            IQueryable<User> query = _context.Users
                .Include(u => u.Roles.Where(ur => ur.DeletedAt == null))
                    .ThenInclude(ur => ur.Role)
                .Include(u => u.Teams.Where(ut => ut.DeletedAt == null))
                    .ThenInclude(ut => ut.Team)
                .Include(u => u.Step);

            if (!includeDeleted)
                query = query.Where(u => u.DeletedAt == null);

            if(!string.IsNullOrEmpty(globalSearchTerm))
            {
                query = query.Where(u => u.Name.ToUpper().Contains(globalSearchTerm.ToUpper()) || 
                                         u.NormalizedEmail!.Contains(globalSearchTerm.ToUpper()) || 
                                         u.Step.Name.ToUpper().Contains(globalSearchTerm) || 
                                         u.Roles.Any(ur => ur.Role.Name.ToUpper().Contains(globalSearchTerm)));
            }

            if (!string.IsNullOrEmpty(email))
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

        public async Task<(List<User> Data, int TotalCount)> GetAllUsersFilteredAsync(string? email, QueryParams? parameters)
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

        public async Task<User?> GetUserByIdAsync(Guid id, bool includeDeleted = false,  bool includeDeletedUserTeams = false)
        {
            IQueryable<User> query = _context.Users
                .Include(u => u.Roles)
                    .ThenInclude(ur => ur.Role)
                .Include(u => u.Step);

            if (!includeDeleted)
                query = query.Where(u => u.DeletedAt == null);

            if(!includeDeletedUserTeams)
                query = query.Include(u => u.Teams.Where(ut => ut.DeletedAt == null))
                    .ThenInclude(ut => ut.Team);
            else
                query = query.Include(u => u.Teams)
                   .ThenInclude(ut => ut.Team);

            query = GetUsersWithIncludes(query);

            return await query.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<List<string>> GetUserRolesByEmailAsync(string email)
        {
            var roles = await _context.Users
                .Where(u => u.Email == email && u.DeletedAt == null)
                .SelectMany(u => u.Roles
                    .Where(ur => ur.DeletedAt == null)
                    .Select(ur => ur.Role.Name))  // ia doar numele rolului
                .ToListAsync();

            return roles;
        }

        public async Task<List<User>> GetUsersByTeamIdAsync(Guid teamId, bool includeDeleted = false)
        {
            IQueryable<User> query = _context.Users;

            if(!includeDeleted)
                query = query.Where(u => u.DeletedAt == null);

            query = GetUsersWithIncludes(query).Where(u => u.Teams.Any(ut => ut.TeamId == teamId));

            return await query.ToListAsync();
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
                .Include(u => u.Teams.Where(ut => ut.DeletedAt == null))
                    .ThenInclude(ut => ut.Team)
                .Include(u => u.Step)
                .Include(u => u.Roles.Where(ur => ur.DeletedAt == null))
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync();
        }

        public async Task<List<User>> GetUsersByStepIdAsync(Guid stepId)
        {
            return await _context.Steps
                .Where(s => s.DeletedAt == null && s.Id == stepId)
                .Include(s => s.Users)
                    .ThenInclude(u => u.Teams.Where(ut => ut.DeletedAt == null))
                        .ThenInclude(ut => ut.Team)
                .Include(s => s.Users)
                    .ThenInclude(u => u.Roles.Where(ur => ur.DeletedAt == null))
                    .ThenInclude(ur => ur.Role)
                .SelectMany(s => s.Users)
                .ToListAsync();
        }

        public async Task<bool> VerifyIfAssigned(Guid userId)
        {
            bool isAssignedToSteps = await _context.Steps
                .AnyAsync(s => s.Users.Any(u => u.Id == userId && u.DeletedAt == null));
            return isAssignedToSteps;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<(List<User> Data, int TotalCount)> GetUnassignedModeratorsByStepIdQueriedAsync(Guid moderatorId, Guid stepId, string? globalSearchTerm, QueryParams? parameters)
        {
            IQueryable<User> query = _context.Users
                .Where(u => u.DeletedAt == null &&
                            !u.Teams.Any() &&
                            u.Roles.Any(ur => ur.DeletedAt == null && ur.RoleId == moderatorId) &&
                            u.StepId == stepId)
                .Include(u => u.Step);


            if (!string.IsNullOrEmpty(globalSearchTerm))
            {
                query = query.Where(u => u.Name.ToUpper().Contains(globalSearchTerm.ToUpper()) ||
                                         u.NormalizedEmail!.Contains(globalSearchTerm.ToUpper()));
            }

            int totalCount = await query.CountAsync();

            if (parameters == null)
            {
                var data = await query.ToListAsync();
                return (data, totalCount);
            }

            // sorting
            if (parameters.SortBy != null)
            {
                if (parameters.SortDescending is bool SortDesc)
                    query = query.ApplySorting<User>(parameters.SortBy, SortDesc);
                else
                    query = query.ApplySorting<User>(parameters.SortBy, false);
            }

            // pagination
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

        public async Task<(List<User> Data, int TotalCount)> GetAllUsersByStepQueriedAsync(Guid stepId, string? globalSearchTerm, QueryParams? parameters)
        {
            IQueryable<User> query = _context.Users
                .Where(u => u.DeletedAt == null);

            if(stepId != Guid.Empty)
                query = query.Where(u => u.StepId == stepId);

            query = GetUsersWithIncludes(query);

            int totalCount = await query.CountAsync();

            if (!string.IsNullOrEmpty(globalSearchTerm))
            {
                query = query.Where(u => u.Name.ToUpper().Contains(globalSearchTerm.ToUpper()) ||
                                         u.NormalizedEmail!.Contains(globalSearchTerm.ToUpper()) || 
                                         u.Step.Name.ToUpper().Contains(globalSearchTerm.ToUpper()));
            }

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
    }
}