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
using FlowManager.Shared.DTOs.Requests;
using FlowManager.Domain.Dtos;

namespace FlowManager.Application.Services
{
    public class TeamService : ITeamService
    {
        private readonly ITeamRepository _teamRepository;
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;

        public TeamService(ITeamRepository teamRepository, IUserRepository userRepository, IRoleRepository roleRepository)
        {
            _teamRepository = teamRepository;
            _userRepository = userRepository;
            _roleRepository = roleRepository;
        }

        public async Task<PagedResponseDto<TeamResponseDto>> GetAllTeamsQueriedAsync(QueriedTeamRequestDto payload)
        {
            (List<Team> result, int totalCount) = await _teamRepository.GetAllTeamsQueriedAsync(
                payload.GlobalSearchTerm,
                payload.Name,
                payload.QueryParams?.ToQueryParams());

            return new PagedResponseDto<TeamResponseDto>
            {
                Data = result.Select(t => new TeamResponseDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Users = t.Users?.Select(ut => new UserResponseDto
                    {
                        Id = ut.User.Id,
                        Name = ut.User.Name,
                        Email = ut.User.Email,
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
                Users = team.Users?.Select(ut => new UserResponseDto
                {
                    Id = ut.User.Id,
                    Name = ut.User.Name,
                    Email = ut.User.Email,
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

                    user.Teams.Add(new UserTeam
                    {
                        TeamId = teamToAdd.Id,
                        UserId = userId,
                    });
                }
            }

            await _teamRepository.AddTeamAsync(teamToAdd);

            return new TeamResponseDto
            {
                Id = teamToAdd.Id,
                Name = teamToAdd.Name,
                Users = teamToAdd.Users?.Select(ut => new UserResponseDto
                {
                    Id = ut.User.Id,
                    Name = ut.User.Name,
                    Email = ut.User.Email,
                }).ToList(),
                UsersCount = teamToAdd.Users?.Count(u => u.DeletedAt == null) ?? 0,
                CreatedAt = teamToAdd.CreatedAt,
                UpdatedAt = teamToAdd.UpdatedAt,
                DeletedAt = teamToAdd.DeletedAt,
            };
        }

        public async Task<TeamResponseDto> UpdateTeamAsync(Guid id, PatchTeamRequestDto payload)
        {
            var teamToUpdate = await _teamRepository.GetTeamByIdAsync(id, includeDeletedUserTeams: true);
            if (teamToUpdate == null)
            {
                throw new EntryNotFoundException($"Team with id {id} was not found.");
            }

            if (!string.IsNullOrEmpty(payload.Name))
            {
                teamToUpdate.Name = payload.Name;
            }

            if (payload.UserIds != null)
            {
                foreach (UserTeam userTeam in teamToUpdate.Users)
                {
                    userTeam.DeletedAt = DateTime.UtcNow;
                }

                foreach (Guid userId in payload.UserIds)
                {
                    var user = await _userRepository.GetUserByIdAsync(userId);
                    if (user == null)
                    {
                        throw new EntryNotFoundException($"User with id {userId} was not found (trying to update team).");
                    }

                    UserTeam existingUserTeam = teamToUpdate.Users.FirstOrDefault(ut => ut.UserId == userId);
                    if (existingUserTeam != null)
                    {
                        existingUserTeam.DeletedAt = null;
                    }
                    else
                    {
                        UserTeam userTeamToAdd = new UserTeam
                        {
                            TeamId = id,
                            UserId = userId
                        };
                        teamToUpdate.Users.Add(userTeamToAdd);
                    }
                }
            }

            teamToUpdate.UpdatedAt = DateTime.UtcNow;
            await _teamRepository.SaveChangesAsync();

            return new TeamResponseDto
            {
                Id = teamToUpdate.Id,
                Name = teamToUpdate.Name,
                Users = teamToUpdate.Users?.Where(ut => ut.DeletedAt == null).Select(ut => new UserResponseDto
                {
                    Id = ut.User.Id,
                    Name = ut.User.Name,
                    Email = ut.User.Email,
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
                UserTeam userTeamToDelete = user.Teams.First(ut => ut.TeamId == id);
                userTeamToDelete.DeletedAt = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;
            }

            teamToDelete.DeletedAt = DateTime.UtcNow;
            teamToDelete.UpdatedAt = DateTime.UtcNow;

            await _teamRepository.SaveChangesAsync();

            return new TeamResponseDto
            {
                Id = teamToDelete.Id,
                Name = teamToDelete.Name,
                Users = teamToDelete.Users?.Select(ut => new UserResponseDto
                {
                    Id = ut.User.Id,
                    Name = ut.User.Name,
                    Email = ut.User.Email,
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
                Users = teamToRestore.Users?.Select(ut => new UserResponseDto
                {
                    Id = ut.User.Id,
                    Name = ut.User.Name,
                    Email = ut.User.Email,
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
                Users = team.Users?.Select(ut => new UserResponseDto
                {
                    Id = ut.User.Id,
                    Name = ut.User.Name,
                    Email = ut.User.Email,
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
                Users = team.Users?.Select(ut => new UserResponseDto 
                {
                    Id = ut.User.Id,
                    Name = ut.User.Name,
                    Email = ut.User.Email,
                    UserName = ut.User.UserName,
                }).ToList()
            };
        }

        public async Task<SplitUsersByTeamIdResponseDto> GetSplitUsersByTeamIdAsync(Guid teamId, QueriedTeamRequestDto payload)
        {
            var team = await _teamRepository.GetTeamWithUsersAsync(teamId);

            if (team == null)
            {
                throw new EntryNotFoundException($"Team with id {teamId} was not found.");
            }

            (List<User> assignedToTeam, List<User> unassignedToTeam) = 
                await _teamRepository.GetSplitUsersByTeamIdQueriedAsync(teamId, payload.GlobalSearchTerm, payload.QueryParams?.ToQueryParams());

            return new SplitUsersByTeamIdResponseDto
            {
                TeamId = teamId,
                AssignedToTeamUsers = assignedToTeam.Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email
                }).ToList(),
                UnassignedToTeamUsers = unassignedToTeam.Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email
                }).ToList(),
                TotalCountAssigned = assignedToTeam.Count,
                TotalCountUnassigned = unassignedToTeam.Count,
                TotalPages = (int)Math.Ceiling((double)((assignedToTeam.Count + unassignedToTeam.Count)) / (payload.QueryParams?.PageSize ?? 1))
            };
        }

        public async Task<PagedResponseDto<TeamResponseDto>> GetAllModeratorTeamsQueriedAsync(Guid stepId, QueriedTeamRequestDto payload)
        {
            QueryParams? parameters = payload.QueryParams?.ToQueryParams();
            Guid moderatorId = (await _roleRepository.GetRoleByRolenameAsync("MODERATOR"))!.Id;

            (List<Team> moderatorTeams, int totalCount) = await _teamRepository.
                GetAllModeratorTeamsByStepIdQueriedAsync(
                stepId,
                moderatorId,
                payload.GlobalSearchTerm,
                parameters);

            return new PagedResponseDto<TeamResponseDto>
            {
                Data = moderatorTeams.Select(t => new TeamResponseDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Users = t.Users.Select(ut => new UserResponseDto
                    {
                        Id = ut.UserId,
                        Name = ut.User.Name,
                        Email = ut.User.Email
                    }).ToList()
                })
            };
        }
    }
}