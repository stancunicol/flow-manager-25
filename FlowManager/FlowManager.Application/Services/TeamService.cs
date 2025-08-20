using FlowManager.Application.Interfaces;
using FlowManager.Domain.Entities;
using FlowManager.Domain.Exceptions;
using FlowManager.Domain.IRepositories;
using FlowManager.Infrastructure.Utils;
using FlowManager.Application.Utils;
using FlowManager.Shared.DTOs.Requests.Team;
using FlowManager.Shared.DTOs.Responses;
using FlowManager.Shared.DTOs.Responses.Team;
using FlowManager.Shared.DTOs.Responses.User;
using FlowManager.Shared.DTOs.Responses.Role;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowManager.Application.IServices;

namespace FlowManager.Application.Services
{
    public class TeamService : ITeamService
    {
        private readonly ITeamRepository _teamRepository;
        private readonly IUserRepository _userRepository;

        public TeamService(ITeamRepository teamRepository, IUserRepository userRepository)
        {
            _teamRepository = teamRepository;
            _userRepository = userRepository;
        }

        public async Task<PagedResponseDto<TeamResponseDto>> GetAllTeamsQueriedAsync(QueriedTeamRequestDto payload)
        {
            (List<Team> result, int totalCount) = await _teamRepository.GetAllTeamsQueriedAsync(
                payload.Name,
                payload.QueryParams?.ToQueryParams());

            return new PagedResponseDto<TeamResponseDto>
            {
                Data = result.Select(t => new TeamResponseDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Users = t.Users?.Select(u => new UserResponseDto
                    {
                        Id = u.Id,
                        Name = u.Name,
                        Email = u.Email,
                    }).ToList(),
                    UsersCount = t.Users?.Count(u => u.DeletedAt == null) ?? 0,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt,
                    DeletedAt = t.DeletedAt,
                }),
                Page = payload.QueryParams?.Page ?? 1,
                PageSize = payload.QueryParams?.PageSize ?? totalCount,
                TotalCount = totalCount
            };
        }

        public async Task<TeamResponseDto> GetTeamByIdAsync(Guid id)
        {
            var team = await _teamRepository.GetTeamByIdAsync(id);

            if (team == null)
            {
                throw new EntryNotFoundException($"Team with id {id} was not found.");
            }

            return new TeamResponseDto
            {
                Id = team.Id,
                Name = team.Name,
                Users = team.Users?.Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                }).ToList(),
                UsersCount = team.Users?.Count(u => u.DeletedAt == null) ?? 0,
                CreatedAt = team.CreatedAt,
                UpdatedAt = team.UpdatedAt,
                DeletedAt = team.DeletedAt,
            };
        }

        public async Task<TeamResponseDto> AddTeamAsync(PostTeamRequestDto payload)
        {
            var existingTeam = await _teamRepository.GetTeamByNameAsync(payload.Name);
            if (existingTeam != null)
            {
                throw new UniqueConstraintViolationException($"Team with name {payload.Name} already exists.");
            }

            Team teamToAdd = new Team
            {
                Name = payload.Name,
            };

            if (payload.UserIds != null && payload.UserIds.Any())
            {
                foreach (Guid userId in payload.UserIds)
                {
                    var user = await _userRepository.GetUserByIdAsync(userId);
                    if (user == null)
                    {
                        throw new EntryNotFoundException($"User with id {userId} was not found (trying to create a team).");
                    }

                    user.TeamId = teamToAdd.Id;
                }
            }

            await _teamRepository.SaveChangesAsync();

            return new TeamResponseDto
            {
                Id = teamToAdd.Id,
                Name = teamToAdd.Name,
                Users = teamToAdd.Users?.Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                }).ToList(),
                UsersCount = teamToAdd.Users?.Count(u => u.DeletedAt == null) ?? 0,
                CreatedAt = teamToAdd.CreatedAt,
                UpdatedAt = teamToAdd.UpdatedAt,
                DeletedAt = teamToAdd.DeletedAt,
            };
        }

        public async Task<TeamResponseDto> UpdateTeamAsync(Guid id, PatchTeamRequestDto payload)
        {
            var teamToUpdate = await _teamRepository.GetTeamByIdAsync(id);

            if (teamToUpdate == null)
            {
                throw new EntryNotFoundException($"Team with id {id} was not found.");
            }

            if (!string.IsNullOrEmpty(payload.Name))
            {
                teamToUpdate.Name = payload.Name;
            }

            teamToUpdate.UpdatedAt = DateTime.UtcNow;

            if (payload.UserIds != null)
            {
                var currentUsers = await _userRepository.GetUsersByTeamIdAsync(id);
                foreach (User user in currentUsers)
                {
                    user.TeamId = null;
                    user.UpdatedAt = DateTime.UtcNow;
                }

                foreach (Guid userId in payload.UserIds)
                {
                    var user = await _userRepository.GetUserByIdAsync(userId);
                    if (user == null)
                    {
                        throw new EntryNotFoundException($"User with id {userId} was not found (trying to update team).");
                    }

                    user.TeamId = id;
                    user.UpdatedAt = DateTime.UtcNow;
                }

                await _userRepository.SaveChangesAsync();
            }

            await _teamRepository.SaveChangesAsync();

            return new TeamResponseDto
            {
                Id = teamToUpdate.Id,
                Name = teamToUpdate.Name,
                Users = teamToUpdate.Users?.Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                }).ToList(),
                UsersCount = teamToUpdate.Users?.Count(u => u.DeletedAt == null) ?? 0,
                CreatedAt = teamToUpdate.CreatedAt,
                UpdatedAt = teamToUpdate.UpdatedAt,
                DeletedAt = teamToUpdate.DeletedAt,
            };
        }

        public async Task<TeamResponseDto> DeleteTeamAsync(Guid id)
        {
            var teamToDelete = await _teamRepository.GetTeamByIdAsync(id);

            if (teamToDelete == null)
            {
                throw new EntryNotFoundException($"Team with id {id} was not found.");
            }

            var users = await _userRepository.GetUsersByTeamIdAsync(id);
            foreach (var user in users)
            {
                user.TeamId = null;
                user.UpdatedAt = DateTime.UtcNow;
            }

            teamToDelete.DeletedAt = DateTime.UtcNow;
            teamToDelete.UpdatedAt = DateTime.UtcNow;

            await _teamRepository.SaveChangesAsync();

            return new TeamResponseDto
            {
                Id = teamToDelete.Id,
                Name = teamToDelete.Name,
                Users = teamToDelete.Users?.Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                }).ToList(),
                UsersCount = teamToDelete.Users?.Count(u => u.DeletedAt == null) ?? 0,
                CreatedAt = teamToDelete.CreatedAt,
                UpdatedAt = teamToDelete.UpdatedAt,
                DeletedAt = teamToDelete.DeletedAt,
            };
        }

        public async Task<TeamResponseDto> RestoreTeamAsync(Guid id)
        {
            var teamToRestore = await _teamRepository.GetTeamByIdAsync(id, includeDeleted: true);

            if (teamToRestore == null)
            {
                throw new EntryNotFoundException($"Team with id {id} was not found.");
            }

            teamToRestore.DeletedAt = null;
            teamToRestore.UpdatedAt = DateTime.UtcNow;

            await _teamRepository.SaveChangesAsync();

            return new TeamResponseDto
            {
                Id = teamToRestore.Id,
                Name = teamToRestore.Name,
                Users = teamToRestore.Users?.Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                }).ToList(),
                UsersCount = teamToRestore.Users?.Count(u => u.DeletedAt == null) ?? 0,
                CreatedAt = teamToRestore.CreatedAt,
                UpdatedAt = teamToRestore.UpdatedAt,
                DeletedAt = teamToRestore.DeletedAt,
            };
        }

        public async Task<TeamResponseDto> GetTeamByNameAsync(string name)
        {
            Team? team = await _teamRepository.GetTeamByNameAsync(name);

            if (team == null)
            {
                throw new EntryNotFoundException($"Team with name {name} was not found.");
            }

            return new TeamResponseDto
            {
                Id = team.Id,
                Name = team.Name,
                Users = team.Users?.Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                }).ToList(),
                UsersCount = team.Users?.Count(u => u.DeletedAt == null) ?? 0,
                CreatedAt = team.CreatedAt,
                UpdatedAt = team.UpdatedAt,
                DeletedAt = team.DeletedAt,
            };
        }

        public async Task<TeamResponseDto> GetTeamWithUsersAsync(Guid id)
        {
            var team = await _teamRepository.GetTeamWithUsersAsync(id);

            if (team == null)
            {
                throw new EntryNotFoundException($"Team with id {id} was not found.");
            }

            return new TeamResponseDto
            {
                Id = team.Id,
                Name = team.Name,
                CreatedAt = team.CreatedAt,
                UpdatedAt = team.UpdatedAt,
                DeletedAt = team.DeletedAt,
                UsersCount = team.Users?.Count ?? 0,
                Users = team.Users?.Select(u => new UserResponseDto 
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    UserName = u.UserName,
                    TeamId = u.TeamId,
                }).ToList()
            };
        }
    }
}