using FlowManager.Application.Interfaces;
using FlowManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using FlowManager.Domain.IRepositories;
using FlowManager.Infrastructure.Utils;
using FlowManager.Domain.Exceptions;
using FlowManager.Domain.Dtos;
using FlowManager.Application.Utils;
using FlowManager.Shared.DTOs.Responses.User;
using FlowManager.Shared.DTOs.Responses;
using FlowManager.Shared.DTOs.Requests.User;

namespace FlowManager.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IEmailService _emailService;

        public UserService(IUserRepository userRepository, IRoleRepository roleRepository)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
        }

        public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllUsersAsync();

            return users.Select(u => new UserResponseDto
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

        public async Task<IEnumerable<UserResponseDto>> GetAllModeratorsAsync()
        {
            var moderators = await _userRepository.GetAllModeratorsAsync();

            return moderators.Select(u => new UserResponseDto
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

        public async Task<IEnumerable<UserResponseDto>> GetAllAdminsAsync()
        {
            var admins = await _userRepository.GetAllAdminsAsync();

            return admins.Select(u => new UserResponseDto
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

        public async Task<PagedResponseDto<UserResponseDto>> GetAllUsersQueriedAsync(QueriedUserRequestDto payload)
        {
            (List<User> result, int totalCount) = await _userRepository.GetAllUsersQueriedAsync(payload.Email, payload.QueryParams.ToQueryParams());

            return new PagedResponseDto<UserResponseDto>
            {
                Data = result.Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    UserName = u.UserName,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt,
                    DeletedAt = u.DeletedAt,
                    Roles = u.Roles.Select(r => r.Role.Name).ToList()
                }),
                Page = payload.QueryParams.Page ?? 1,
                PageSize = payload.QueryParams.PageSize ?? totalCount,
                TotalCount = totalCount
            };
        }

        public async Task<PagedResponseDto<UserResponseDto>> GetAllUsersFilteredAsync(QueriedUserRequestDto payload)
        {
            QueryParams? parameters = payload.QueryParams?.ToQueryParams();
            (List<User> result, int totalCount) = await _userRepository.GetAllUsersFilteredAsync(payload.Email, parameters);

            return new PagedResponseDto<UserResponseDto>
            {
                Data = result.Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    UserName = u.UserName,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt,
                    DeletedAt = u.DeletedAt,
                    Roles = u.Roles.Select(r => r.Role.Name).ToList()
                }),
                Page = payload.QueryParams?.Page ?? 1,
                PageSize = payload.QueryParams?.PageSize ?? totalCount,
                TotalCount = totalCount
            };
        }

        public async Task<UserResponseDto> GetUserByIdAsync(Guid id)
        {
            var user = await _userRepository.GetUserByIdAsync(id);

            if (user == null)
            {
                throw new EntryNotFoundException($"User with id {id} was not found.");
            }

            return new UserResponseDto
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

        public async Task<UserResponseDto> AddUserAsync(PostUserRequestDto payload)
        {
            if(_userRepository.GetUserByEmailAsync(payload.Email) != null)
            {
                throw new UniqueConstraintViolationException($"User with email {payload.Email} already exists.");
            }

            User userToAdd = new User
            {
                UserName = payload.Username,
                NormalizedUserName = payload.Username.ToUpper(),
                Name = payload.Name,
                NormalizedEmail = payload.Email.ToUpper(),
                Email = payload.Email,
                EmailConfirmed = true
            };

            foreach (Guid roleId in payload.Roles)
            {
                if (_roleRepository.GetRoleByIdAsync(roleId) == null)
                {
                    throw new EntryNotFoundException($"Role with id {roleId} was not found (trying to create a user).");
                }

                UserRole userRole = new UserRole
                {
                    UserId = userToAdd.Id,
                    RoleId = roleId
                };
                userToAdd.Roles.Add(userRole);
            }

            var result = await _userRepository.AddUserAsync(userToAdd);

            // Send welcome email
            try
            {
                Console.WriteLine($"Attempting to send welcome email to: {result.Email}");
                await _emailService.SendWelcomeEmailAsync(result.Email, result.Name);
                Console.WriteLine($"Welcome email sent successfully to: {result.Email}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending welcome email: {ex.Message}");
                // Don't throw - just log the error so user creation still succeeds
            }

            return new UserResponseDto
            {
                Id = result.Id,
                Name = result.Name,
                Email = result.Email,
                UserName = result.UserName,
                CreatedAt = result.CreatedAt,
                UpdatedAt = result.UpdatedAt,
                DeletedAt = result.DeletedAt,
            };
        }

        public async Task<UserResponseDto> UpdateUserAsync(Guid id, PatchUserRequestDto payload)
        {
            var userToUpdate = await _userRepository.GetUserByIdAsync(id);

            if (userToUpdate == null)
            {
                throw new EntryNotFoundException($"User with id {id} was not found.");
            }

            PatchHelper.PatchFrom<PatchUserRequestDto, User>(userToUpdate, payload);
            userToUpdate.UpdatedAt = DateTime.UtcNow;

            if (payload.Roles != null)
            {
                foreach (UserRole userRole in userToUpdate.Roles)
                {
                    userRole.DeletedAt = DateTime.UtcNow;
                }

                foreach (Guid roleId in payload.Roles)
                {
                    if(_roleRepository.GetRoleByIdAsync(roleId) == null)
                    {
                        throw new EntryNotFoundException($"Role with id {roleId} was not found (trying to create a user).");
                    }

                    UserRole userRoleToUpdate = userToUpdate.Roles.FirstOrDefault(ur => ur.RoleId == roleId);
                    if (userRoleToUpdate != null)
                    {
                        userRoleToUpdate.DeletedAt = null;
                    }
                    else
                    {
                        UserRole userRoleToAdd = new UserRole
                        {
                            RoleId = roleId,
                            UserId = userToUpdate.Id
                        };
                        userToUpdate.Roles.Add(userRoleToAdd);
                    }
                }
            }

            await _userRepository.SaveChangesAsync();
            return new UserResponseDto
            {
                Id = userToUpdate.Id,
                Name = userToUpdate.Name,
                Email = userToUpdate.Email,
                UserName = userToUpdate.UserName,
                CreatedAt = userToUpdate.CreatedAt,
                UpdatedAt = userToUpdate.UpdatedAt,
                DeletedAt = userToUpdate.DeletedAt,
            };
        }

        public async Task<UserResponseDto> DeleteUserAsync(Guid id)
        {
            var userToDelete = await _userRepository.GetUserByIdAsync(id);

            if (userToDelete == null)
            {
                throw new EntryNotFoundException($"User with id {id} was not found.");
            }

            userToDelete.DeletedAt = DateTime.UtcNow;

            await _userRepository.SaveChangesAsync();
            return new UserResponseDto
            {
                Id = userToDelete.Id,
                Name = userToDelete.Name,
                Email = userToDelete.Email,
                UserName = userToDelete.UserName,
                CreatedAt = userToDelete.CreatedAt,
                UpdatedAt = userToDelete.UpdatedAt,
                DeletedAt = userToDelete.DeletedAt,
            };
        }

        public async Task<UserResponseDto> RestoreUserAsync(Guid id)
        {
            var userToRestore = await _userRepository.GetUserByIdAsync(id, includeDeleted: true);

            if (userToRestore == null)
            {
                throw new EntryNotFoundException($"User with id {id} was not found.");
            }

            userToRestore.DeletedAt = null;

            await _userRepository.SaveChangesAsync();
            return new UserResponseDto
            {
                Id = userToRestore.Id,
                Name = userToRestore.Name,
                Email = userToRestore.Email,
                UserName = userToRestore.UserName,
                CreatedAt = userToRestore.CreatedAt,
                UpdatedAt = userToRestore.UpdatedAt,
                DeletedAt = userToRestore.DeletedAt,
            };
        }

        public async Task<UserResponseDto> GetUserByEmailAsync(string email)
        {
            User? user = await _userRepository.GetUserByEmailAsync(email);

            if (user == null)
            { 
                throw new EntryNotFoundException($"User with email {email} was not found."); 
            }

            return new UserResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                UserName = user.UserName,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                DeletedAt = user.DeletedAt,
            };
        }

        public async Task<bool> ResetPassword(Guid id, string newPassword)
        {
            User? user = await _userRepository.GetUserByIdAsync(id);
            
            if(user == null)
            {
                throw new EntryNotFoundException($"User with id {id} was not found.");
            }  
             
            user.PasswordHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(newPassword)));
            await _userRepository.SaveChangesAsync();
            return true;
        }
    }
}