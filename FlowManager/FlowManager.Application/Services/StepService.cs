using FlowManager.Shared.DTOs.Requests.Step;
using FlowManager.Shared.DTOs.Responses;
using FlowManager.Shared.DTOs.Responses.Step;
using FlowManager.Application.Interfaces;
using FlowManager.Domain.Dtos;
using FlowManager.Domain.Entities;
using FlowManager.Domain.Exceptions;
using FlowManager.Domain.IRepositories;
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

                    foreach (var user in team.Users)
                    {
                        if (!stepToPatch.Users.Any(u => u.Id == user.UserId))
                        {
                            stepToPatch.Users.Add(user.User);
                        }
                    }
                }
            }

            await _stepRepository.SaveChangesAsync();

            return new StepResponseDto
            {
                Id = stepToPatch.Id,
                Name = stepToPatch.Name,
                Users = stepToPatch.Users.Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                }).ToList(),
                Teams = stepToPatch.Users
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
                var userDtos = step.Users?.Select(u => new UserResponseDto
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
                }).ToList() ?? new List<UserResponseDto>();

                var teamDtos = userDtos
                                .SelectMany(u => u.Teams)
                                .GroupBy(t => t.Id)
                                .Select(g => g.First())
                                .ToList();

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