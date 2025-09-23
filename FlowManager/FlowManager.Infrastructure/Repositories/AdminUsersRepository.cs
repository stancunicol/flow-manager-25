using FlowManager.Domain.Entities;
using FlowManager.Domain.IRepositories;
using FlowManager.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace FlowManager.Infrastructure.Repositories
{
    public class AdminUsersRepository : IAdminUsersRepository
    {
        private readonly AppDbContext _context;

        public AdminUsersRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<User>> GetUsersForImpersonationAsync(string? currentUserId, string? search, int page, int pageSize)
        {
            var query = _context.Users
                .Include(u => u.Step)
                .Include(u => u.Roles)
                .ThenInclude(ur => ur.Role)
                .Where(u => u.DeletedAt == null);

            if (!string.IsNullOrEmpty(currentUserId))
            {
                query = query.Where(u => u.Id.ToString() != currentUserId);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchTerm = search.Trim().ToLower();
                query = query.Where(u =>
                    u.Name.ToLower().Contains(searchTerm) ||
                    (u.Email != null && u.Email.ToLower().Contains(searchTerm)));
            }

            return await query
                .OrderBy(u => u.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}