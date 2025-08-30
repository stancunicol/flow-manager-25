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
                Users = s.Users.Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                }).ToList(),
                Teams = s.Users.SelectMany(u => u.Teams).Select(ut => new TeamResponseDto
                {
                    Id = ut.Team.Id,
                    Name = ut.Team.Name,
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
                Teams = stepToPost.Users.SelectMany(u => u.Teams).Select(ut => new TeamResponseDto
                {
                    Id = ut.Team.Id,
                    Name = ut.Team.Name,
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

                stepToPatch.Users.Clear();

                foreach (Guid userId in payload.UserIds)
                {
                    User existingUser = (await _userRepository.GetUserByIdAsync(userId))!;
                    stepToPatch.Users.Add(existingUser);
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
                Teams = stepToPatch.Users.SelectMany(u => u.Teams).Select(ut => new TeamResponseDto
                {
                    Id = ut.Team.Id,
                    Name = ut.Team.Name,
                }).ToList(),
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
                Teams = stepToDelete.Users.SelectMany(u => u.Teams).Select(ut => new TeamResponseDto
                {
                    Id = ut.Team.Id,
                    Name = ut.Team.Name,
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

            (List<Step> data, int totalCount) = await _stepRepository.GetAllStepsIncludeUsersAndTeamsQueriedAsync(payload.Name, parameters);

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
                    Teams = step.Users.SelectMany(u => u.Teams).Select(ut => new TeamResponseDto
                    {
                        Id = ut.Team.Id,
                        Name = ut.Team.Name,
                    }).ToList(),
                }).ToList(),
                TotalCount = totalCount,
                Page = parameters?.Page ?? 1,
                PageSize = parameters?.PageSize ?? totalCount,
            };
        }
    }
}