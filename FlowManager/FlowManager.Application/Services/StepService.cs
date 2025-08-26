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
            Step? step = await _stepRepository.GetStepByIdAsync(id);

            if (step == null)
            {
                throw new EntryNotFoundException($"Step with id {id} was not found.");
            }

            return new StepResponseDto
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
            Step? stepToPatch = await _stepRepository.GetStepByIdAsync(id);

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
                    if (_userRepository.GetUserByIdAsync(userId) == null)
                    {
                        throw new EntryNotFoundException($"User with id {userId} was not found.");
                    }
                }

                foreach (StepUser stepUser in stepToPatch.Users)
                {
                    stepUser.DeletedAt = DateTime.UtcNow;
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
                    if (_teamRepository.GetTeamWithUsersAsync(teamId) == null)
                    {
                        throw new EntryNotFoundException($"Team with id {teamId} was not found.");
                    }
                }

                foreach (StepTeam stepTeam in stepToPatch.Teams)
                {
                    stepTeam.DeletedAt = DateTime.UtcNow;
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

        public async Task<StepResponseDto> AssignUserToStepAsync(Guid id, Guid userId)
        {
            User? user = await _userRepository.GetUserByIdAsync(userId);

            if (user == null)
            {
                throw new EntryNotFoundException($"User with id {userId} was not found.");
            }


            Step? step = await _stepRepository.GetStepByIdAsync(id);

            if (step == null)
            {
                throw new EntryNotFoundException($"Step with id {id} was not found.");
            }

            if (step.Users.FirstOrDefault(su => su.UserId == userId) != null)
            {
                throw new UniqueConstraintViolationException($"Relationship between user id {user.Id} and step id {step.Id} already exists.");
            }

            step.Users.Add(new StepUser
            {
                UserId = userId,
                User = user,
                StepId = step.Id
            });

            await _stepRepository.SaveChangesAsync();

            return new StepResponseDto
            {
                Id = step.Id,
                Name = step.Name,
                Users = step.Users.Where(su => su.User != null).Select(su => new UserResponseDto
                {
                    Id = su.UserId,
                    Name = su.User.Name,
                    Email = su.User.Email,
                }).ToList(),
                Teams = step.Teams.Select(st => new TeamResponseDto
                {
                    Id = st.TeamId,
                    Name = st.Team.Name,
                    Users = st.Team.Users.Select(ut => new UserResponseDto
                    {
                        Id = ut.User.Id,
                        Name = ut.User.Name,
                        Email = ut.User.Email
                    }).ToList()
                }).ToList(),
            };
        }

        public async Task<StepResponseDto> UnassignUserFromStepAsync(Guid id, Guid userId)
        {
            User? user = await _userRepository.GetUserByIdAsync(userId, includeDeleted: true);

            if (user == null)
            {
                throw new EntryNotFoundException($"User with id {userId} was not found.");
            }

            Step? step = await _stepRepository.GetStepByIdAsync(id);

            if (step == null)
            {
                throw new EntryNotFoundException($"Step with id {id} was not found.");
            }

            StepUser? stepUserToUnassign = step.Users.FirstOrDefault(su => su.UserId == userId && su.DeletedAt == null);
            if (stepUserToUnassign == null)
            {
                throw new EntryNotFoundException($"Relationship between user id {user.Id} and step id {step.Id} does not exist.");
            }

            stepUserToUnassign.DeletedAt = DateTime.UtcNow;

            await _userRepository.SaveChangesAsync();

            return new StepResponseDto
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