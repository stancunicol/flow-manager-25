using FlowManager.Application.DTOs;
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

        public async Task<User?> AddUserAsync(User user)
        {
            _context.Users.Add(user);
            await SaveChangesAsync();
            return user;
        }

        public async Task<IEnumerable<User>> GetAllAdminsAsync()
        {
            var admins = await _context.Users
                .Include(u => u.Roles)
                .ThenInclude(ur => ur.Role)
                .Where(u => u.DeletedAt == null && u.Roles.Any(r => r.Role.Name == "Admin"))
                .ToListAsync();

            return admins;
        }

        public async Task<IEnumerable<User>> GetAllModeratorsAsync()
        {
            var moderators = await _context.Users
                .Include(u => u.Roles)
                .ThenInclude(ur => ur.Role)
                .Where(u => u.DeletedAt == null && u.Roles.Any(r => r.Role.Name == "Moderator"))
                .ToListAsync();

            return moderators;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            var users = await _context.Users
                .Include(u => u.Roles)
                .ThenInclude(ur => ur.Role)
                .Where(u => u.DeletedAt == null)
                .ToListAsync();

            return users;
        }

        public async Task<(List<User> Data, int TotalCount)> GetAllUsersQueriedAsync(string? email, QueryParams parameters)
        {
            IQueryable<User> query = _context.Users
                .Where(u => u.DeletedAt == null)
                .Include(u => u.Roles)
                .ThenInclude(ur => ur.Role);

            if (email != null)
            {
                query = query.Where(u => u.NormalizedEmail.Contains(email.ToUpper()));
            }

            int totalCount = await query.CountAsync();

            if (parameters == null)
            {
                var data = await query.ToListAsync();
                return (data, totalCount);
            }

            if (parameters.SortBy != null)
            {
                if (parameters.SortDescending is bool SortDesc)
                    query = query.ApplySorting<User>(parameters.SortBy, SortDesc);
                else
                    query = query.ApplySorting<User>(parameters.SortBy, false);
            }

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
                .Where(u => u.DeletedAt == null)
                .Include(u => u.Roles)
                .ThenInclude(ur => ur.Role);

            // filtering 
            if (!string.IsNullOrEmpty(email))
            {
                query = query.Where(u => u.NormalizedEmail.Contains(email.ToUpper()));
            }

            int totalCount = await query.CountAsync();

            if (parameters == null)
            {
                return (await query.ToListAsync(), totalCount);
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

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users.Where(u => u.DeletedAt == null)
                .FirstOrDefaultAsync(u => u.NormalizedEmail == email.ToUpper());
        }

        public async Task<User?> GetUserByIdAsync(Guid id, bool includeDeleted = false)
        {
            IQueryable<User> query = _context.Users;

            if (!includeDeleted)
                query = query.Where(u => u.DeletedAt == null);

            query = query
                .Include(u => u.Roles)
                .ThenInclude(ur => ur.Role);

            return await query.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}