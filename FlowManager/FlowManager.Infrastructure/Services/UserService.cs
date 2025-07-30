using FlowManager.Application.Interfaces;
using FlowManager.Domain.Entities;
using FlowManager.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;

namespace FlowManager.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public UserService(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        public async Task<IEnumerable<User>> GetAllUsers()
        {
            return await _context.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
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

        public async Task<bool> UpdateUserProfile(Guid id, string name, string email)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return false;

            var emailTaken = await _userManager.Users
                .AnyAsync(u => u.Email == email && u.Id != id);

            if (emailTaken) return false;

            user.Name = name;
            user.UpdatedAt = DateTime.UtcNow;

           // await _userManager.SetUserNameAsync(user, username);
            await _userManager.SetEmailAsync(user, email);

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
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
