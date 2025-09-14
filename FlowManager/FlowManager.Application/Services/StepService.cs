using FlowManager.Shared.DTOs.Requests.Step;
using FlowManager.Shared.DTOs.Responses;
using FlowManager.Shared.DTOs.Responses.Step;
using FlowManager.Application.Interfaces;
using FlowManager.Domain.Dtos;
using FlowManager.Domain.Entities;
using FlowManager.Domain.Exceptions;
using FlowManager.Domain.IRepositories;
using System.Linq;
using FlowManager.Application.Utils;
using FlowManager.Shared.DTOs.Responses.User;
using FlowManager.Shared.DTOs.Responses.Team;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FlowManager.Application.Services
{
    public class StepService : IStepService
    {
        private readonly IStepRepository _stepRepository;
        private readonly IUserRepository _userRepository;
        private readonly IFlowRepository _flowRepository;
        private readonly ITeamRepository _teamRepository;
        private readonly IRoleRepository _roleRepository;

        public StepService(IStepRepository stepRepository, 
            IUserRepository userRepository,
            IFlowRepository flowRepository,
            ITeamRepository teamRepository,
            IRoleRepository roleRepository)
        {
            _stepRepository = stepRepository;
            _userRepository = userRepository;
            _flowRepository = flowRepository;
            _teamRepository = teamRepository;
            _roleRepository = roleRepository;
        }

        public async Task<List<StepResponseDto>> GetStepsAsync()
        {
            var steps = await _stepRepository.GetStepsAsync();

            return steps.Select(s => new StepResponseDto
            {
                Id = s.Id,
                Name = s.Name,
                Users = s.Users.Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                }).ToList(),
                Teams = s.Users
            .SelectMany(u => u.Teams)
            .Select(ut => ut.Team)
            .Where(t => t != null)
            .GroupBy(t => t!.Id)
            .Select(g => new TeamResponseDto
            {
                Id = g.Key,
                Name = g.First()!.Name
            })
            .ToList()
            }).ToList();
        }

        public async Task<StepResponseDto> GetStepAsync(Guid id)
        {
            Step? step = await _stepRepository.GetStepByIdAsync(id, includeUsers: true, includeTeams: true);

            if (step == null)
            {
                throw new EntryNotFoundException($"Step with id {id} was not found.");
            }

            return new StepResponseDto
            {
                Id = step.Id,
                Name = step.Name,

                Users = step.Users
        .Where(u => u.Teams == null || !u.Teams.Any())
        .Select(u => new UserResponseDto
        {
            Id = u.Id,
            Name = u.Name,
            Email = u.Email,
        })
        .ToList(),

                Teams = step.Users
        .SelectMany(u => u.Teams)
        .Select(ut => ut.Team)
        .Where(t => t != null)
        .GroupBy(t => t!.Id)
        .Select(g => new TeamResponseDto
        {
            Id = g.Key,
            Name = g.First()!.Name,
            Users = g.SelectMany(t => t!.Users.Select(ut => new UserResponseDto
            {
                Id = ut.User.Id,
                Name = ut.User.Name,
                Email = ut.User.Email
            })).DistinctBy(u => u.Id).ToList()
        })
        .ToList(),

                DeletedAt = step.DeletedAt
            };
        }

        public async Task<StepResponseDto> PostStepAsync(PostStepRequestDto payload)
        {
            Step stepToPost = new Step
            {
                Name = payload.Name
            };

            foreach (Guid userId in payload.UserIds)
            {
                User? user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    throw new EntryNotFoundException($"User with id {userId} not found.");
                }
                else
                {
                    stepToPost.Users.Add(user);
                }
            }

            foreach (Guid flowId in payload.FlowIds)
            {
                Flow? flow = await _flowRepository.GetFlowByIdAsync(flowId);
                if (flow == null)
                {
                    throw new EntryNotFoundException($"Flow with id {flowId} not found.");
                }
                else
                {
                    stepToPost.FlowSteps.Add(new FlowStep
                    {
                        FlowId = flowId,
                        StepId = stepToPost.Id
                    });
                }
            }

            await _stepRepository.PostStepAsync(stepToPost);

            return new StepResponseDto
            {
                Id = stepToPost.Id,
                Name = stepToPost.Name,
                Users = stepToPost.Users.Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                }).ToList(),
                Teams = stepToPost.Users
            .SelectMany(u => u.Teams)
            .Select(ut => ut.Team)
            .Where(t => t != null)
            .GroupBy(t => t!.Id)
            .Select(g => new TeamResponseDto
            {
                Id = g.Key,
                Name = g.First()!.Name
            })
            .ToList()
            };
        }

        public async Task<StepResponseDto> PatchStepAsync(Guid id, PatchStepRequestDto payload)
        {
            Step? stepToPatch = await _stepRepository.GetStepByIdAsync(
                id,
                includeDeletedStepUser: true,
                includeDeletedStepTeams: true
            );
            if (stepToPatch == null)
            {
                throw new EntryNotFoundException($"Step with id {id} was not found.");
            }

            if (!string.IsNullOrEmpty(payload.Name))
            {
                stepToPatch.Name = payload.Name;
            }

            if (payload.UserIds != null)
            {
                stepToPatch.Users.Clear();
                foreach (Guid userId in payload.UserIds)
                {
                    var user = await _userRepository.GetUserByIdAsync(userId);
                    if (user == null)
                    {
                        throw new EntryNotFoundException($"User with id {userId} was not found.");
                    }
                    stepToPatch.Users.Add(user);
                }
            }

            if (payload.TeamIds != null && payload.TeamIds.Any())
            {
                foreach (Guid teamId in payload.TeamIds)
                {
                    var team = await _teamRepository.GetTeamByIdAsync(teamId);
                    if (team == null)
                    {
                        throw new EntryNotFoundException($"Team with id {teamId} was not found.");
                    }
                    foreach (var userTeam in team.Users)
                    {
                        if (!stepToPatch.Users.Any(u => u.Id == userTeam.UserId))
                        {
                            stepToPatch.Users.Add(userTeam.User);
                        }
                    }
                }
            }

            await _stepRepository.SaveChangesAsync();

            // Create user DTOs with their teams
            var userDtos = stepToPatch.Users.Select(u => new UserResponseDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Teams = u.Teams?
                    .Where(ut => ut.Team != null)
                    .Select(ut => new TeamResponseDto
                    {
                        Id = ut.Team!.Id,
                        Name = ut.Team.Name
                    }).ToList() ?? new List<TeamResponseDto>()
            }).ToList();

            var teamDtos = userDtos
                .SelectMany(u => u.Teams)
                .GroupBy(t => t.Id)
                .Select(teamGroup =>
                {
                    var team = teamGroup.First();
                    var teamUsers = userDtos
                        .Where(u => u.Teams.Any(t => t.Id == team.Id))
                        .Select(u => new UserResponseDto
                        {
                            Id = u.Id,
                            Name = u.Name,
                            Email = u.Email,
                            Teams = new List<TeamResponseDto>()
                        })
                        .ToList();

                    return new TeamResponseDto
                    {
                        Id = team.Id,
                        Name = team.Name,
                        Users = teamUsers
                    };
                })
                .ToList();

            return new StepResponseDto
            {
                Id = stepToPatch.Id,
                Name = stepToPatch.Name,
                Users = userDtos,
                Teams = teamDtos
            };
        }

        public async Task<StepResponseDto> DeleteStepAsync(Guid id)
        {
            Step? stepToDelete = await _stepRepository.GetStepByIdAsync(id);

            if (stepToDelete == null)
            {
                throw new EntryNotFoundException($"Step with id {id} was not found.");
            }

            await _stepRepository.DeleteStepAsync(stepToDelete);

            return new StepResponseDto
            {
                Id = stepToDelete.Id,
                Name = stepToDelete.Name,
                Users = stepToDelete.Users.Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                }).ToList(),
                Teams = stepToDelete.Users
            .SelectMany(u => u.Teams)
            .Select(ut => ut.Team)
            .Where(t => t != null)
            .GroupBy(t => t!.Id)
            .Select(g => new TeamResponseDto
            {
                Id = g.Key,
                Name = g.First()!.Name
            })
            .ToList()
            };
        }

        public async Task<StepResponseDto> AssignUserToStepAsync(Guid stepId, Guid userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
                throw new EntryNotFoundException($"User with id {userId} was not found.");

            var step = await _stepRepository.GetStepByIdAsync(stepId, includeDeletedStepUser: true, includeUsers: true, includeTeams: true);
            if (step == null)
                throw new EntryNotFoundException($"Step with id {stepId} was not found.");

            var existingActive = step.Users.FirstOrDefault(u => u.Id == userId && u.DeletedAt == null);
            if (existingActive != null)
                throw new UniqueConstraintViolationException($"User {user.Name} is already assigned to step {step.Name}.");

            var existingDeleted = step.Users.FirstOrDefault(u => u.Id == userId && u.DeletedAt != null);
            if (existingDeleted != null)
            {
                existingDeleted.DeletedAt = null;
                step.Users.Add(existingDeleted);
            }
            else
            {
                step.Users.Add(user);
            }

            await _stepRepository.SaveChangesAsync();

            step = await _stepRepository.GetStepByIdAsync(step.Id, includeUsers: true, includeTeams: true);

            return new StepResponseDto
            {
                Id = step.Id,
                Name = step.Name,
                Users = step.Users.Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                }).ToList(),
                Teams = step.Users.SelectMany(u => u.Teams).Select(ut => new TeamResponseDto
                {
                    Id = ut.Team.Id,
                    Name = ut.Team.Name,
                }).ToList(),
            };
        }

        public async Task<StepResponseDto> UnassignUserFromStepAsync(Guid stepId, Guid userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId, includeDeleted: true);
            if (user == null)
                throw new EntryNotFoundException($"User with id {userId} was not found.");

            var step = await _stepRepository.GetStepByIdAsync(
                stepId,
                includeDeletedStepUser: true,
                includeUsers: true,
                includeTeams: true
            );
            if (step == null)
                throw new EntryNotFoundException($"Step with id {stepId} was not found.");

            if (!step.Users.Contains(user))
                throw new EntryNotFoundException($"User {user.Name} is not assigned to step {step.Name}.");
            else
                step.Users.Remove(user);

            await _stepRepository.SaveChangesAsync();

            step = await _stepRepository.GetStepByIdAsync(stepId, includeUsers: true, includeTeams: true);

            return new StepResponseDto
            {
                Id = step.Id,
                Name = step.Name,
                Users = step.Users.Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                }).ToList(),
                Teams = step.Users.SelectMany(u => u.Teams).Select(ut => new TeamResponseDto
                {
                    Id = ut.Team.Id,
                    Name = ut.Team.Name,
                }).ToList(),
            };
        }

        public async Task<PagedResponseDto<StepResponseDto>> GetAllStepsQueriedAsync(QueriedStepRequestDto payload)
        {
            QueryParams? parameters = payload.QueryParams?.ToQueryParams();
            Role? moderatorRole = await _roleRepository.GetRoleByRolenameAsync("MODERATOR");
            if (moderatorRole == null)
                throw new EntryNotFoundException("Moderator role does not exist");

            (List<Step> steps, int totalCount) = await _stepRepository.GetAllStepsQueriedAsync(
                moderatorRole.Id,
                payload.Name,
                parameters
            );

            var stepDtos = steps.Select(step =>
            {
                var userDtos = step.Users?
                    .Where(u => u.Teams == null || !u.Teams.Any())
                    .Select(u => new UserResponseDto
                    {
                        Id = u.Id,
                        Name = u.Name,
                        Email = u.Email,
                        Teams = new List<TeamResponseDto>()
                    }).ToList() ?? new List<UserResponseDto>();

                var teamDtos = step.Users?
                    .SelectMany(u => u.Teams ?? new List<UserTeam>())
                    .Where(ut => ut.Team != null)
                    .GroupBy(ut => ut.Team!.Id)
                    .Select(teamGroup =>
                    {
                        var team = teamGroup.First().Team!;
                        var teamUsers = teamGroup
                            .Select(ut => ut.User)
                            .Distinct()
                            .Select(u => new UserResponseDto
                            {
                                Id = u.Id,
                                Name = u.Name,
                                Email = u.Email,
                                Teams = new List<TeamResponseDto>()
                            })
                            .ToList();

                        return new TeamResponseDto
                        {
                            Id = team.Id,
                            Name = team.Name,
                            Users = teamUsers
                        };
                    })
                    .ToList() ?? new List<TeamResponseDto>();

                return new StepResponseDto
                {
                    Id = step.Id,
                    Name = step.Name,
                    Users = userDtos,
                    Teams = teamDtos
                };
            }).ToList();

            return new PagedResponseDto<StepResponseDto>
            {
                Data = stepDtos,
                TotalCount = totalCount,
                Page = parameters?.Page ?? 1,
                PageSize = parameters?.PageSize ?? totalCount
            };
        }

        public async Task<PagedResponseDto<StepResponseDto>> GetAllStepsIncludeUsersAndTeamsQueriedAsync(QueriedStepRequestDto payload)
        {
            QueryParams? parameters = payload.QueryParams?.ToQueryParams();

            Role? moderatorRole = await _roleRepository.GetRoleByRolenameAsync("MODERATOR");

            if (moderatorRole == null)
            {
                throw new EntryNotFoundException("Moderator role does not exist");
            }

            (List<Step> data, int totalCount) = await _stepRepository.GetAllStepsIncludeUsersAndTeamsQueriedAsync(moderatorRole.Id, payload.Name, parameters);

            return new PagedResponseDto<StepResponseDto>
            {
                Data = data.Select(step => new StepResponseDto
                {
                    Id = step.Id,
                    Name = step.Name,
                    Users = step.Users.Select(u => new UserResponseDto
                    {
                        Id = u.Id,
                        Name = u.Name,
                        Email = u.Email,
                    }).ToList(),
                    Teams = step.Users
            .SelectMany(u => u.Teams)
            .Select(ut => ut.Team)
            .Where(t => t != null)
            .GroupBy(t => t!.Id)
            .Select(g => new TeamResponseDto
            {
                Id = g.Key,
                Name = g.First()!.Name
            })
            .ToList()
                }),
                TotalCount = totalCount,
                Page = parameters?.Page ?? 1,
                PageSize = parameters?.PageSize ?? totalCount,
            };
        }
    }
}