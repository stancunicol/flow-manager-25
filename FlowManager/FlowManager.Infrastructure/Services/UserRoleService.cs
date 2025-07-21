using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowManager.Application.Interfaces;
using FlowManager.Domain.Entities;
using FlowManager.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FlowManager.Infrastructure.Services
{
    public class UserRoleService
    {
       
            private readonly AppDbContext _context;

            public UserRoleService(AppDbContext context)
            {
                _context = context;
            }

            public async Task<IEnumerable<UserRole>> GetAllUserRoles()
            {
                return await _context.UserRoles
                    .Include(ur => ur.User)
                    .ToListAsync();
            }

            public async Task<UserRole?> GetUserRole(Guid id)
            {
                return await _context.UserRoles
                    .Include(ur => ur.User)
                    .FirstOrDefaultAsync(ur => ur.Id == id);
            }

            public async Task<IEnumerable<UserRole>> GetRolesByUser(Guid userId)
            {
                return await _context.UserRoles
                    .Include(ur => ur.User)
                    .Where(ur => ur.UserId == userId)
                    .ToListAsync();
            }

            public async Task<IEnumerable<UserRole>> GetUsersByRole(RoleType roleType)
            {
                return await _context.UserRoles
                    .Include(ur => ur.User)
                    .Where(ur => ur.Role == roleType)
                    .ToListAsync();
            }

            public async Task<bool> UpdateUserRole(Guid id, UserRole userRole)
            {
                if (id != userRole.Id) return false;

                _context.Entry(userRole).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                    return true;
                }
                catch (DbUpdateConcurrencyException)
                {
                    return await _context.UserRoles.AnyAsync(e => e.Id == id);
                }
            }

            public async Task<UserRole?> CreateUserRole(UserRole userRole)
            {
                if (await _context.UserRoles.AnyAsync(ur => ur.UserId == userRole.UserId && ur.Role == userRole.Role))
                {
                    return null;
                }

                userRole.Id = Guid.NewGuid();
                userRole.AssignedAt = DateTime.UtcNow;

                _context.UserRoles.Add(userRole);
                await _context.SaveChangesAsync();

                return userRole;
            }

            public async Task<bool> DeleteUserRole(Guid id)
            {
                var userRole = await _context.UserRoles.FindAsync(id);
                if (userRole == null) return false;

                _context.UserRoles.Remove(userRole);
                await _context.SaveChangesAsync();
                return true;
            }
        }
 }
