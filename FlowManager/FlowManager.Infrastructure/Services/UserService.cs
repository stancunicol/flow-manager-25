using FlowManager.Application.Interfaces;
using FlowManager.Application.DTOs;
using FlowManager.Domain.Entities;
using FlowManager.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace FlowManager.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _context.Users
                .Include(u => u.Roles)
                .ThenInclude(ur => ur.Role)
                .Where(u => u.DeletedAt == null)
                .ToListAsync();

            return users.Select(u => new UserDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                UserName = u.UserName,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt,
                DeletedAt = u.DeletedAt,
                Roles = u.Roles.Select(r => r.Role.Name).ToList()
            });
        }

        public async Task<IEnumerable<UserDto>> GetAllModeratorsAsync()
        {
            var moderators = await _context.Users
                .Include(u => u.Roles)
                .ThenInclude(ur => ur.Role)
                .Where(u => u.DeletedAt == null && u.Roles.Any(r => r.Role.Name == "Moderator"))
                .ToListAsync();

            return moderators.Select(u => new UserDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                UserName = u.UserName,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt,
                DeletedAt = u.DeletedAt,
                Roles = u.Roles.Select(r => r.Role.Name).ToList()
            });
        }

        public async Task<IEnumerable<UserDto>> GetAllAdminsAsync()
        {
            var admins = await _context.Users
                .Include(u => u.Roles)
                .ThenInclude(ur => ur.Role)
                .Where(u => u.DeletedAt == null && u.Roles.Any(r => r.Role.Name == "Admin"))
                .ToListAsync();

            return admins.Select(u => new UserDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                UserName = u.UserName,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt,
                DeletedAt = u.DeletedAt,
                Roles = u.Roles.Select(r => r.Role.Name).ToList()
            });
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersFilteredAsync(string? searchTerm = null, string? role = null)
        {
            var query = _context.Users
                .Include(u => u.Roles)
                .ThenInclude(ur => ur.Role)
                .Where(u => u.DeletedAt == null);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(u => u.Name.Contains(searchTerm) ||
                                       u.Email.Contains(searchTerm) ||
                                       u.UserName.Contains(searchTerm));
            }

            if (!string.IsNullOrEmpty(role))
            {
                query = query.Where(u => u.Roles.Any(r => r.Role.Name == role));
            }

            var users = await query.ToListAsync();

            return users.Select(u => new UserDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                UserName = u.UserName,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt,
                DeletedAt = u.DeletedAt,
                Roles = u.Roles.Select(r => r.Role.Name).ToList()
            });
        }

        public async Task<UserDto?> GetUserByIdAsync(Guid id)
        {
            var user = await _context.Users
                .Include(u => u.Roles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == id && u.DeletedAt == null);

            if (user == null) return null;

            return new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                UserName = user.UserName,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                DeletedAt = user.DeletedAt,
                Roles = user.Roles.Select(r => r.Role.Name).ToList()
            };
        }

        public async Task<UserDto?> AddUserAsync(CreateUserDto createUserDto)
        {
            var user = new User
            {
                Name = createUserDto.Name,
                Email = createUserDto.Email,
                UserName = createUserDto.UserName,
                PasswordHash = null,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            foreach (var roleName in createUserDto.Roles)
            {
                var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
                if (role != null)
                {
                    _context.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
                }
            }
            await _context.SaveChangesAsync();

            //TO DO: Trimite email ul de creare cont

            return new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                UserName = user.UserName,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                DeletedAt = user.DeletedAt,
                Roles = createUserDto.Roles
            };
        }

        public async Task<bool> UpdateUserAsync(Guid id, UpdateUserDto updateUserDto)
        {
            var user = await _context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Id == id && u.DeletedAt == null);

            if (user == null) return false;

            if (!string.IsNullOrEmpty(updateUserDto.Name))
                user.Name = updateUserDto.Name;

            if (!string.IsNullOrEmpty(updateUserDto.Email))
                user.Email = updateUserDto.Email;

            if (!string.IsNullOrEmpty(updateUserDto.UserName))
                user.UserName = updateUserDto.UserName;

            user.UpdatedAt = DateTime.UtcNow;

            if (updateUserDto.Roles != null)
            {
                _context.UserRoles.RemoveRange(user.Roles);

                foreach (var roleName in updateUserDto.Roles)
                {
                    var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
                    if (role != null)
                    {
                        _context.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
                    }
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteUserAsync(Guid id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && u.DeletedAt == null);
            if (user == null) return false;

            user.DeletedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RestoreUserAsync(Guid id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && u.DeletedAt != null);
            if (user == null) return false;

            user.DeletedAt = null;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}