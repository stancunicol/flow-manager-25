using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowManager.Application.IServices;
using FlowManager.Domain.Dtos;
using FlowManager.Domain.Entities;
using FlowManager.Domain.IRepositories;
using FlowManager.Shared.DTOs.Requests.Flow;
using FlowManager.Shared.DTOs.Responses.Flow;
using FlowManager.Shared.DTOs.Responses.Step;
using FlowManager.Shared.DTOs.Responses;
using FlowManager.Shared.DTOs.Responses.FlowStep;
using FlowManager.Application.Utils;
using FlowManager.Domain.Exceptions;
using FlowManager.Shared.DTOs.Requests.FlowStep;
using FlowManager.Shared.DTOs.Responses.Team;
using FlowManager.Shared.DTOs.Responses.User;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowManager.Application.Services
{
    public class FlowStepService : IFlowStepService
    {
        private readonly IFlowRepository _flowRepository;
        private readonly IStepRepository _stepRepository;
        private readonly IFlowStepRepository _flowStepRepository;
        private readonly IUserRepository _userRepository;
        private readonly ITeamRepository _teamRepository;

        public FlowStepService(IFlowRepository flowRepository, IStepRepository stepRepository, IFlowStepRepository flowStepRepository, IUserRepository userRepository, ITeamRepository teamRepository)
        {
            _flowRepository = flowRepository;
            _stepRepository = stepRepository;
            _flowStepRepository = flowStepRepository;
            _userRepository = userRepository;
            _teamRepository = teamRepository;
        }

        public async Task<PagedResponseDto<FlowStepResponseDto>> GetAllFlowStepsQueriedAsync(QueriedFlowStepRequestDto payload)
        {
            QueryParams? parameters = payload.QueryParams?.ToQueryParams();
            (List<FlowStep> data, int totalCount) = await _flowStepRepository.GetAllFlowStepsQueriedAsync(payload.Name, parameters);

            return new PagedResponseDto<FlowStepResponseDto>
            {
                Data = data.Select(f => new FlowStepResponseDto
                {
                    Id = f.Id,
                    FlowId = f.FlowId,
                    StepId = f.StepId,
                    StepName = f.Step.Name,
                    IsApproved = f.IsApproved,
                    CreatedAt = f.CreatedAt,
                    UpdatedAt = f.UpdatedAt,
                    DeletedAt = f.DeletedAt
                }).ToList(),
                TotalCount = totalCount
            };
        }

        public async Task<List<FlowStepResponseDto>> GetAllFlowStepsAsync()
        {
            var flowSteps = await _flowStepRepository.GetAllFlowStepsAsync();

            return flowSteps.Select(fs => new FlowStepResponseDto
            {
                Id = fs.Id,
                FlowId = fs.FlowId,
                StepId = fs.StepId,
                IsApproved = fs.IsApproved,
                CreatedAt = fs.CreatedAt,
                UpdatedAt = fs.UpdatedAt,
                DeletedAt = fs.DeletedAt
            }).ToList();
        }

        public async Task<FlowStepResponseDto?> GetFlowStepByIdAsync(Guid id)
        {
            var flowStep = await _flowStepRepository.GetFlowStepByIdAsync(id);

            if (flowStep == null)
            {
                throw new EntryNotFoundException($"FlowStep with id {id} was not found.");
            }

            List<UserResponseDto> userList = new List<UserResponseDto>();
            List<TeamResponseDto> teamList = new List<TeamResponseDto>();

            if (flowStep.AssignedUsers != null)
            {
                foreach (var user in flowStep.AssignedUsers)
                {
                    userList.Add(new UserResponseDto
                    {
                        Id = user.UserId,
                        Name = user.User?.Name,
                        Email = user.User?.Email
                    });
                }
            }

            if (flowStep.AssignedTeams != null)
            {
                foreach (var team in flowStep.AssignedTeams)
                {
                    teamList.Add(new TeamResponseDto
                    {
                        Id = team.TeamId,
                        Name = team.Team.Name,
                        Users = team.Team.Users.Select(u => new UserResponseDto
                        {
                            Id = u.User.Id,
                            Name = u.User.Name,
                            Email = u.User.Email
                        }).ToList()
                    });
                }
            }

            return (new FlowStepResponseDto
            {
                Id = flowStep.Id,
                FlowId = flowStep.FlowId,
                StepId = flowStep.StepId,
                Users = userList,
                Teams = teamList,
                IsApproved = flowStep.IsApproved,
                CreatedAt = flowStep.CreatedAt,
                UpdatedAt = flowStep.UpdatedAt,
                DeletedAt = flowStep.DeletedAt
            });
        }

        public async Task<FlowStepResponseDto> CreateFlowStepAsync(PostFlowStepRequestDto flowStep)
        {
            var department = await _stepRepository.GetStepByIdAsync(flowStep.StepId);
            if (department == null)
            {
                throw new EntryNotFoundException($"Department with id {flowStep.StepId} not found.");
            }

            FlowStep flowStepToPost = new FlowStep
            {
                FlowId = Guid.Empty,
                StepId = flowStep.StepId,
                IsApproved = false
            };

            foreach (Guid userId in flowStep.UserIds)
            {
                User? user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    throw new EntryNotFoundException($"User with id {userId} not found.");
                }
                else
                {
                    flowStepToPost.AssignedUsers.Add(new FlowStepUser
                    {
                        FlowStepId = flowStepToPost.Id,
                        UserId = userId
                    });
                }
            }

            foreach (Guid teamId in flowStep.TeamIds)
            {
                Team? team = await _teamRepository.GetTeamByIdAsync(teamId);
                if (team == null)
                {
                    throw new EntryNotFoundException($"Team with id {teamId} not found.");
                }
                else
                {
                    flowStepToPost.AssignedTeams.Add(new FlowStepTeam
                    {
                        FlowStepId = flowStepToPost.Id,
                        TeamId = teamId
                    });
                }
            }

            await _flowStepRepository.AddFlowStepAsync(flowStepToPost);

            return new FlowStepResponseDto
            {
                Id = flowStepToPost.Id,
                FlowId = Guid.Empty,
                StepId = flowStep.StepId,
                IsApproved = flowStepToPost.IsApproved,
                Users = flowStepToPost.AssignedUsers.Select(su => new UserResponseDto
                {
                    Id = su.UserId,
                    Name = su.User.Name,
                    Email = su.User.Email,
                }).ToList(),
                Teams = flowStepToPost.AssignedTeams.Select(st => new TeamResponseDto
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

        public async Task<FlowStepResponseDto> UpdateFlowStepAsync(Guid id, PatchFlowStepRequestDto payload)
        {
            var flowStep = await _flowStepRepository.GetFlowStepByIdAsync(id, includeDeletedFlowStepUsers: true, includeDeletedFlowStepTeams: true);

            if (flowStep == null)
            {
                throw new EntryNotFoundException($"FlowStep with id {id} not found.");
            }

            if (payload.IsApproved.HasValue)
            {
                flowStep.IsApproved = payload.IsApproved.Value;
            }

            if (payload.UserIds != null && payload.UserIds.Any())
            {
                foreach (var userId in payload.UserIds)
                {
                    var user = await _userRepository.GetUserByIdAsync(userId);
                    if (user == null)
                    {
                        throw new EntryNotFoundException($"User with id {userId} was not found.");
                    }
                }

                await _flowStepRepository.UpdateFlowStepUsersAsync(flowStep.Id, payload.UserIds);
            }

            if (payload.TeamIds != null && payload.TeamIds.Any())
            {
                foreach (var teamId in payload.TeamIds)
                {
                    var team = await _teamRepository.GetTeamWithUsersAsync(teamId);
                    if (team == null)
                    {
                        throw new EntryNotFoundException($"Team with id {teamId} was not found.");
                    }
                }

                await _flowStepRepository.UpdateFlowStepTeamsAsync(flowStep.Id, payload.TeamIds);
            }

            await _flowStepRepository.UpdateFlowStepAsync(flowStep);

            var updatedFlowStep = await _flowStepRepository.GetFlowStepByIdAsync(id);

            return new FlowStepResponseDto
            {
                Id = updatedFlowStep.Id,
                FlowId = updatedFlowStep.FlowId,
                StepId = updatedFlowStep.StepId,
                IsApproved = updatedFlowStep.IsApproved,
                Users = updatedFlowStep.AssignedUsers
                    .Where(fsu => fsu.DeletedAt == null)
                    .Select(fsu => new UserResponseDto
                    {
                        Id = fsu.UserId,
                        Name = fsu.User?.Name,
                        Email = fsu.User?.Email
                    }).ToList(),
                Teams = updatedFlowStep.AssignedTeams
                    .Where(fst => fst.DeletedAt == null)
                    .Select(fst => new TeamResponseDto
                    {
                        Id = fst.TeamId,
                        Name = fst.Team?.Name,
                        Users = fst.Team?.Users
                            .Where(tu => tu.DeletedAt == null)
                            .Select(tu => new UserResponseDto
                            {
                                Id = tu.User.Id,
                                Name = tu.User.Name,
                                Email = tu.User.Email
                            }).ToList() ?? new List<UserResponseDto>()
                    }).ToList(),
                UpdatedAt = updatedFlowStep.UpdatedAt
            };
        }

        public async Task<FlowStepResponseDto> DeleteFlowStepAsync(Guid id)
        {
            var flowStep = await _flowStepRepository.GetFlowStepByIdAsync(id, includeDeletedFlowStepUsers: true, includeDeletedFlowStepTeams: true);
            if (flowStep == null)
            {
                throw new EntryNotFoundException($"FlowStep with id {id} not found.");
            }

            await _flowStepRepository.DeleteFlowStepAsync(flowStep);

            return new FlowStepResponseDto
            {
                Id = flowStep.Id,
                FlowId = flowStep.FlowId,
                StepId = flowStep.StepId,
                IsApproved = flowStep.IsApproved,
                Users = flowStep.AssignedUsers
                    .Where(fsu => fsu.DeletedAt == null)
                    .Select(fsu => new UserResponseDto
                    {
                        Id = fsu.UserId,
                        Name = fsu.User?.Name,
                        Email = fsu.User?.Email
                    }).ToList(),
                Teams = flowStep.AssignedTeams
                    .Where(fst => fst.DeletedAt == null)
                    .Select(fst => new TeamResponseDto
                    {
                        Id = fst.TeamId,
                        Name = fst.Team?.Name,
                        Users = fst.Team?.Users
                            .Where(tu => tu.DeletedAt == null)
                            .Select(tu => new UserResponseDto
                            {
                                Id = tu.User.Id,
                                Name = tu.User.Name,
                                Email = tu.User.Email
                            }).ToList() ?? new List<UserResponseDto>()
                    }).ToList(),
                DeletedAt = flowStep.UpdatedAt
            };
        }
    }
}
