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

        public StepService(IStepRepository stepRepository, IUserRepository userRepository, IFlowRepository flowRepository, ITeamRepository teamRepository)
        {
            _stepRepository = stepRepository;
            _userRepository = userRepository;
            _flowRepository = flowRepository;
            _teamRepository = teamRepository;
        }

        public async Task<List<StepResponseDto>> GetStepsAsync()
        {
            var steps = await _stepRepository.GetStepsAsync();

            return steps.Select(s => new StepResponseDto
            {
                Id = s.Id,
                Name = s.Name,
                Users = s.Users.Select(su => new UserResponseDto
                {
                    Id = su.UserId,
                    Name = su.User.Name,
                    Email = su.User.Email,
                }).ToList(),
                Teams = s.Teams.Select(st => new TeamResponseDto
                {
                    Id = st.TeamId,
                    Name = st.Team.Name,
                    Users = st.Team.Users.Select(u => new UserResponseDto
                    {
                        Id = u.User.Id,
                        Name = u.User.Name,
                        Email = u.User.Email
                    }).ToList()
                }).ToList(),
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
                DeletedAt = step.DeletedAt,
                Users = step.Users.Select(su => new UserResponseDto
                {
                    Id = su.UserId,
                    Name = su.User.Name,
                    Email = su.User.Email,
                }).ToList(),
                Teams = step.Teams.Select(st => new TeamResponseDto
                {
                    Id = st.TeamId,
                    Name = st.Team.Name,
                    Users = st.Team.Users.Select(u => new UserResponseDto
                    {
                        Id = u.User.Id,
                        Name = u.User.Name,
                        Email = u.User.Email
                    }).ToList()
                }).ToList(),
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
                    stepToPost.Users.Add(new StepUser
                    {
                        StepId = stepToPost.Id,
                        UserId = userId
                    });
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

            foreach (Guid teamId in payload.TeamIds)
            {
                Team? team = await _teamRepository.GetTeamByIdAsync(teamId);
                if (team == null)
                {
                    throw new EntryNotFoundException($"Team with id {teamId} not found.");
                }
                else
                {
                    stepToPost.Teams.Add(new StepTeam
                    {
                        StepId = stepToPost.Id,
                        TeamId = teamId
                    });
                }
            }

            await _stepRepository.PostStepAsync(stepToPost);

            return new StepResponseDto
            {
                Id = stepToPost.Id,
                Name = stepToPost.Name,
                Users = stepToPost.Users.Select(su => new UserResponseDto
                {
                    Id = su.UserId,
                    Name = su.User.Name,
                    Email = su.User.Email,
                }).ToList(),
                Teams = stepToPost.Teams.Select(st => new TeamResponseDto
                {
                    Id = st.TeamId,
                    Name = st.Team.Name,
                    Users = st.Team.Users.Select(u => new UserResponseDto
                    {
                        Id = u.User.Id,
                        Name = u.User.Name,
                        Email = u.User.Email
                    }).ToList()
                }).ToList(),
            };
        }

        public async Task<StepResponseDto> PatchStepAsync(Guid id, PatchStepRequestDto payload)
        {
            Step? stepToPatch = await _stepRepository.GetStepByIdAsync(id, includeDeletedStepUser: true, includeDeletedStepTeams: true);

            if (stepToPatch == null)
            {
                throw new EntryNotFoundException($"Step with id {id} was not found.");
            }

            if (!string.IsNullOrEmpty(payload.Name))
            {
                stepToPatch.Name = payload.Name;
            }

            if (payload.UserIds != null && payload.UserIds.Any())
            {
                foreach (Guid userId in payload.UserIds)
                {
                    var user = await _userRepository.GetUserByIdAsync(userId);
                    if (user == null)
                    {
                        throw new EntryNotFoundException($"User with id {userId} was not found.");
                    }
                }

                foreach (StepUser stepUser in stepToPatch.Users)
                {
                    if (stepUser.DeletedAt == null)
                    {
                        stepUser.DeletedAt = DateTime.UtcNow;
                    }
                }

                foreach (Guid userId in payload.UserIds)
                {
                    if (stepToPatch.Users.Any(su => su.UserId == userId))
                    {
                        StepUser existingUser = stepToPatch.Users.First(su => su.UserId == userId);
                        existingUser!.DeletedAt = null;
                    }
                    else
                    {
                        stepToPatch.Users.Add(new StepUser
                        {
                            UserId = userId,
                            StepId = stepToPatch.Id
                        });
                    }
                }
            }

            if (payload.TeamIds != null && payload.TeamIds.Any())
            {
                foreach (Guid teamId in payload.TeamIds)
                {
                    var team = await _teamRepository.GetTeamWithUsersAsync(teamId);
                    if (team == null)
                    {
                        throw new EntryNotFoundException($"Team with id {teamId} was not found.");
                    }
                }

                foreach (StepTeam stepTeam in stepToPatch.Teams)
                {
                    if (stepTeam.DeletedAt == null)
                    {
                        stepTeam.DeletedAt = DateTime.UtcNow;
                    }
                }

                foreach (Guid teamId in payload.TeamIds)
                {
                    if (stepToPatch.Teams.Any(st => st.TeamId == teamId))
                    {
                        StepTeam existingTeam = stepToPatch.Teams.First(st => st.TeamId == teamId);
                        existingTeam!.DeletedAt = null;
                    }
                    else
                    {
                        stepToPatch.Teams.Add(new StepTeam
                        {
                            TeamId = teamId,
                            StepId = stepToPatch.Id
                        });
                    }
                }
            }

            await _stepRepository.SaveChangesAsync();

            return new StepResponseDto
            {
                Id = stepToPatch.Id,
                Name = stepToPatch.Name,
                Users = stepToPatch.Users.Select(su => new UserResponseDto
                {
                    Id = su.UserId,
                    Name = su.User.Name,
                    Email = su.User.Email,
                }).ToList(),
                Teams = stepToPatch.Teams.Select(st => new TeamResponseDto
                {
                    Id = st.TeamId,
                    Name = st.Team.Name,
                    Users = st.Team.Users.Select(u => new UserResponseDto
                    {
                        Id = u.User.Id,
                        Name = u.User.Name,
                        Email = u.User.Email
                    }).ToList()
                }).ToList(),
            };
        }

        public async Task<StepResponseDto> DeleteStepAsync(Guid id)
        {
            Step? step = await _stepRepository.GetStepByIdAsync(id);

            if (step == null)
            {
                throw new EntryNotFoundException($"Step with id {id} was not found.");
            }

            await _stepRepository.DeleteStepAsync(step);

            return new StepResponseDto
            {
                Id = step.Id,
                Name = step.Name,
                DeletedAt = step.DeletedAt,
                Users = step.Users.Select(su => new UserResponseDto
                {
                    Id = su.UserId,
                    Name = su.User.Name,
                    Email = su.User.Email,
                }).ToList(),
                Teams = step.Teams.Select(st => new TeamResponseDto
                {
                    Id = st.TeamId,
                    Name = st.Team.Name,
                    Users = st.Team.Users.Select(u => new UserResponseDto
                    {
                        Id = u.User.Id,
                        Name = u.User.Name,
                        Email = u.User.Email
                    }).ToList()
                }).ToList(),
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

            var existingActive = step.Users.FirstOrDefault(su => su.UserId == userId && su.DeletedAt == null);
            if (existingActive != null)
                throw new UniqueConstraintViolationException($"User {user.Name} is already assigned to step {step.Name}.");

            var existingDeleted = step.Users.FirstOrDefault(su => su.UserId == userId && su.DeletedAt != null);
            if (existingDeleted != null)
            {
                _stepRepository.AttachStepUser(existingDeleted);
                existingDeleted.DeletedAt = null;
            }
            else
            {
                await _stepRepository.AddStepUserAsync(new StepUser
                {
                    StepId = step.Id,
                    UserId = userId
                });
            }

            await _stepRepository.SaveChangesAsync();

            step = await _stepRepository.GetStepByIdAsync(step.Id, includeUsers: true, includeTeams: true);

            return new StepResponseDto
            {
                Id = step.Id,
                Name = step.Name,
                Users = step.Users
                    .Where(su => su.DeletedAt == null && su.User != null)
                    .Select(su => new UserResponseDto
                    {
                        Id = su.UserId,
                        Name = su.User.Name,
                        Email = su.User.Email
                    }).ToList(),
                Teams = step.Teams
                    .Where(st => st.DeletedAt == null)
                    .Select(st => new TeamResponseDto
                    {
                        Id = st.TeamId,
                        Name = st.Team.Name,
                        Users = st.Team.Users
                            .Where(ut => ut.User != null)
                            .Select(ut => new UserResponseDto
                            {
                                Id = ut.User.Id,
                                Name = ut.User.Name,
                                Email = ut.User.Email
                            }).ToList()
                    }).ToList()
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

            var stepUser = step.Users.FirstOrDefault(su => su.UserId == userId && su.DeletedAt == null);
            if (stepUser == null)
                throw new EntryNotFoundException($"User {user.Name} is not assigned to step {step.Name}.");

            stepUser.DeletedAt = DateTime.UtcNow;

            await _stepRepository.SaveChangesAsync();

            Console.WriteLine($"User {userId} unassigned from step {stepId} at {stepUser.DeletedAt}");

            step = await _stepRepository.GetStepByIdAsync(stepId, includeUsers: true, includeTeams: true);

            return new StepResponseDto
            {
                Id = step.Id,
                Name = step.Name,
                DeletedAt = DateTime.UtcNow,
                Users = step.Users
                    .Where(su => su.DeletedAt == null && su.User != null)
                    .Select(su => new UserResponseDto
                    {
                        Id = su.UserId,
                        Name = su.User.Name,
                        Email = su.User.Email
                    }).ToList(),
                Teams = step.Teams
                    .Where(st => st.DeletedAt == null)
                    .Select(st => new TeamResponseDto
                    {
                        Id = st.TeamId,
                        Name = st.Team.Name,
                        Users = st.Team.Users
                            .Where(ut => ut.User != null)
                            .Select(ut => new UserResponseDto
                            {
                                Id = ut.User.Id,
                                Name = ut.User.Name,
                                Email = ut.User.Email
                            }).ToList()
                    }).ToList()
            };
        }

        public async Task<PagedResponseDto<StepResponseDto>> GetAllStepsQueriedAsync(QueriedStepRequestDto payload)
        {
            QueryParams? parameters = payload.QueryParams?.ToQueryParams();

            (List<Step> data, int totalCount) = await _stepRepository.GetAllStepsIncludeUsersAndTeamsQueriedAsync(payload.Name, parameters);

            return new PagedResponseDto<StepResponseDto>
            {
                Data = data.Select(step => new StepResponseDto
                {
                    Id = step.Id,
                    Name = step.Name,
                    Users = step.Users.Select(su => new UserResponseDto
                    {
                        Id = su.UserId,
                        Name = su.User.Name,
                        Email = su.User.Email,
                    }).ToList(),
                    Teams = step.Teams.Select(st => new TeamResponseDto
                    {
                        Id = st.TeamId,
                        Name = st.Team.Name,
                        Users = st.Team.Users.Select(u => new UserResponseDto
                        {
                            Id = u.User.Id,
                            Name = u.User.Name,
                            Email = u.User.Email
                        }).ToList()
                    }).ToList(),
                }).ToList(),
                TotalCount = totalCount,
                Page = parameters?.Page ?? 1,
                PageSize = parameters?.PageSize ?? totalCount,
            };
        }
    }
}