using FlowManager.Application.Interfaces;
using FlowManager.Application.IServices;
using FlowManager.Application.Utils;
using FlowManager.Domain.Dtos;
using FlowManager.Domain.Entities;
using FlowManager.Domain.Exceptions;
using FlowManager.Domain.IRepositories;
using FlowManager.Infrastructure.Utils;
using FlowManager.Shared.DTOs.Requests.User;
using FlowManager.Shared.DTOs.Responses;
using FlowManager.Shared.DTOs.Responses.Role;
using FlowManager.Shared.DTOs.Responses.Team;
using FlowManager.Shared.DTOs.Responses.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace FlowManager.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IEmailService _emailService;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly ITeamRepository _teamRepostiory;

        public UserService(IUserRepository userRepository,
            IRoleRepository roleRepository,
            IEmailService emailService, 
            IPasswordHasher<User> password,
            ITeamRepository teamRepository)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _emailService = emailService;
            _passwordHasher = password;
            _teamRepostiory = teamRepository;
        }

        private UserResponseDto MapToUserResponseDto(User user)
        {
            return new UserResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                UserName = user.UserName,
                Teams = user.Teams.Select(ut => new TeamResponseDto
                {
                    Id = ut.Team.Id,
                    Name = ut.Team.Name,
                }).ToList(),
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                DeletedAt = user.DeletedAt,
                Roles = user.Roles.Select(r => new RoleResponseDto
                {
                    Id = r.RoleId,
                    Name = r.Role.Name
                }).ToList()
            };
        }

        public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllUsersAsync();
            return users.Select(MapToUserResponseDto);
        }

        public async Task<IEnumerable<UserResponseDto>> GetAllModeratorsAsync()
        {
            var moderators = await _userRepository.GetAllModeratorsAsync();
            return moderators.Select(MapToUserResponseDto);
        }

        public async Task<IEnumerable<UserResponseDto>> GetAllAdminsAsync()
        {
            var admins = await _userRepository.GetAllAdminsAsync();
            return admins.Select(MapToUserResponseDto);
        }

        public async Task<PagedResponseDto<UserResponseDto>> GetAllUsersQueriedAsync(QueriedUserRequestDto payload)
        {
            (List<User> result, int totalCount) = await _userRepository.GetAllUsersQueriedAsync(payload.Email, payload.GlobalSearchTerm,
                payload.QueryParams?.ToQueryParams(), includeDeleted: true); 

            return new PagedResponseDto<UserResponseDto>
            {
                Data = result.Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    UserName = u.UserName,
                    Teams = u.Teams.Select(ut => new TeamResponseDto
                    {
                        Id = ut.Team.Id,
                        Name = ut.Team.Name,
                    }).ToList(),
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt,
                    DeletedAt = u.DeletedAt,
                    Roles = u.Roles.Select(r => new RoleResponseDto
                    {
                        Id = r.RoleId,
                        Name = r.Role.Name 
                    }).ToList()
                }),
                Page = payload.QueryParams?.Page ?? 1,
                PageSize = payload.QueryParams?.PageSize ?? totalCount,
                TotalCount = totalCount
            };
        }

        public async Task<PagedResponseDto<UserResponseDto>> GetAllUsersFilteredAsync(QueriedUserRequestDto payload)
        {
            QueryParams? parameters = payload.QueryParams?.ToQueryParams();
            (List<User> result, int totalCount) = await _userRepository.GetAllUsersFilteredAsync(payload.Email, parameters);

            return new PagedResponseDto<UserResponseDto>
            {
                Data = result.Select(MapToUserResponseDto),
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

            return MapToUserResponseDto(user);
        }

        public async Task<UserResponseDto> AddUserAsync(PostUserRequestDto payload)
        {
            if (await _userRepository.GetUserByEmailAsync(payload.Email) != null)
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
                EmailConfirmed = false,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString()
            };

            foreach(Guid teamId in payload.TeamsIds)
            {
                if(await _teamRepostiory.GetTeamByIdAsync(teamId) == null)
                {
                    throw new EntryNotFoundException($"Team with id {teamId} was not found (trying to create a new user).");
                }

                UserTeam userTeam = new UserTeam
                {
                    UserId = userToAdd.Id,
                    TeamId = teamId,
                };

                userToAdd.Teams.Add(userTeam);
            }

            foreach (Guid roleId in payload.Roles)
            {
                if (await _roleRepository.GetRoleByIdAsync(roleId) == null)
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

            Guid basicRoleId = (await _roleRepository.GetRoleByRolenameAsync("Basic"))!.Id;

            UserRole basicRole = new UserRole
            {
                UserId = userToAdd.Id,
                RoleId = basicRoleId
            };

            userToAdd.Roles.Add(basicRole);

            // Send welcome email
            try
            {
                Console.WriteLine($"Attempting to send welcome email to: {userToAdd.Email}");
                await _emailService.SendWelcomeEmailAsync(userToAdd.Email, userToAdd.Name);
                Console.WriteLine($"Welcome email sent successfully to: {userToAdd.Email}");
            }
            catch (Exception ex)
            {
                throw new EmailNotSentException($"Failed to send welcome email to {userToAdd.Email}. Invalid email address.");
            }


            var result = await _userRepository.AddUserAsync(userToAdd);

            return new UserResponseDto
            {
                Id = result.Id,
                Name = result.Name,
                Email = result.Email,
                UserName = result.UserName,
                Teams = result.Teams.Select(ut => new TeamResponseDto
                {
                    Id = ut.TeamId,
                    Name = ut.Team.Name
                }).ToList(),
                CreatedAt = result.CreatedAt,
                UpdatedAt = result.UpdatedAt,
                DeletedAt = result.DeletedAt,
                Roles = result.Roles.Select(r => new RoleResponseDto
                {
                    Id = r.RoleId,
                    Name = r.Role.Name
                }).ToList()
            };
        }

        public async Task<UserResponseDto> UpdateUserAsync(Guid id, PatchUserRequestDto payload)
        {
            var userToUpdate = await _userRepository.GetUserByIdAsync(id, includeDeletedUserTeams: true);

            if (userToUpdate == null)
            {
                throw new EntryNotFoundException($"User with id {id} was not found.");
            }

            if(!string.IsNullOrEmpty(payload.Name))
            {
                userToUpdate.Name = payload.Name;
            }

            if(!string.IsNullOrEmpty(payload.Email))
            {
                userToUpdate.Email = payload.Email;
                userToUpdate.UserName = payload.Email;
            }

            if (payload.TeamsIds != null && payload.TeamsIds.Count() != 0)
            {
                foreach(UserTeam userTeam in userToUpdate.Teams)
                {
                    userTeam.DeletedAt = DateTime.UtcNow;
                }

                foreach(Guid teamId in payload.TeamsIds)
                {
                    if(await _teamRepostiory.GetTeamByIdAsync(teamId) == null)
                    {
                        UserTeam existingUserTeam = userToUpdate.Teams.First(userToUpdate => userToUpdate.TeamId == teamId);
                        existingUserTeam.DeletedAt = null;
                    }
                    else
                    {
                        UserTeam userTeamToAdd = new UserTeam
                        {
                            TeamId = teamId,
                            UserId = userToUpdate.Id
                        };
                        userToUpdate.Teams.Add(userTeamToAdd);
                    }
                }
            }

            if (payload.Roles != null)
            {
                foreach (UserRole userRole in userToUpdate.Roles)
                {
                    userRole.DeletedAt = DateTime.UtcNow;
                }

                foreach (Guid roleId in payload.Roles)
                {
                    if (await _roleRepository.GetRoleByIdAsync(roleId) == null)
                    {
                        throw new EntryNotFoundException($"Role with id {roleId} was not found (trying to update a user).");
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

            userToUpdate.UpdatedAt = DateTime.UtcNow;

            await _userRepository.SaveChangesAsync();
            return MapToUserResponseDto(userToUpdate);
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
            return MapToUserResponseDto(userToDelete);
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
            return MapToUserResponseDto(userToRestore);
        }

        public async Task<UserResponseDto> GetUserByEmailAsync(string email)
        {
            User? user = await _userRepository.GetUserByEmailAsync(email);

            if (user == null)
            {
                throw new EntryNotFoundException($"User with email {email} was not found.");
            }

            return MapToUserResponseDto(user);
        }

        public async Task<bool> ResetPassword(Guid id, string newPassword)
        {
            User? user = await _userRepository.GetUserByIdAsync(id);

            if (user == null)
            {
                throw new EntryNotFoundException($"User with id {id} was not found.");
            }

            user.PasswordHash = _passwordHasher.HashPassword(user, newPassword);
            await _userRepository.SaveChangesAsync();
            return true;
        }

        public async Task<List<string>> GetUserRolesByEmailAsync(string email)
        {
            List<string> roles = await _userRepository.GetUserRolesByEmailAsync(email);
            if (roles.Count() == 0)
            {
                throw new EntryNotFoundException($"Roles for user with email {email} were not found.");
            }
            return roles;
        }

        public async Task<bool> VerifyIfAssignedAsync(Guid id)
        {
            bool assigned = await _userRepository.VerifyIfAssigned(id);
            return assigned;
        }

        public async Task<bool> AssignUserToStepAsync(Guid stepId, Guid userId)
        {
            bool assigned = await _userRepository.AssignUserToStepAsync(stepId, userId);
            return assigned;
        }
    }
}