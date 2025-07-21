using FlowManager.Application.Interfaces;
using FlowManager.Domain.Entities;
using FlowManager.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;

namespace FlowManager.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetAllUsers()
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                .Include(u => u.Forms)
                .Include(u => u.StepUsers)
                .Include(u => u.UpdateHistories)
                .ToListAsync();
        }

        public async Task<User?> GetUserById(Guid id)
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                .Include(u => u.Forms)
                .Include(u => u.StepUsers)
                .Include(u => u.UpdateHistories)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetUserByEmail(string email)
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                .Include(u => u.Forms)
                .Include(u => u.StepUsers)
                .Include(u => u.UpdateHistories)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> CreateUser(User user)
        {
            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
            {
                return null;
            }

            user.Id = Guid.NewGuid();
            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> UpdateUser(Guid id, User user)
        {
            if (id != user.Id) return false;

            user.UpdatedAt = DateTime.UtcNow;
            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                return await _context.Users.AnyAsync(u => u.Id == id);
            }
        }

        public async Task<bool> DeleteUser(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
