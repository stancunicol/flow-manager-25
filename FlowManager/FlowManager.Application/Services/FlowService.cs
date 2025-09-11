using FlowManager.Application.Interfaces;
using FlowManager.Domain.Dtos;
using FlowManager.Domain.Entities;
using FlowManager.Domain.Exceptions;
using FlowManager.Domain.IRepositories;
using FlowManager.Shared.DTOs.Requests.Flow;
using FlowManager.Shared.DTOs.Responses;
using FlowManager.Shared.DTOs.Responses.Flow;
using FlowManager.Application.Utils;
using Microsoft.EntityFrameworkCore;
using FlowManager.Shared.DTOs.Responses.Step;
using FlowManager.Shared.DTOs.Responses.FormTemplate;
using FlowManager.Shared.DTOs.Responses.FormTemplateComponent;
using FlowManager.Shared.DTOs.Requests.FlowStep;
using FlowManager.Shared.DTOs.Responses.FlowStep;
using FlowManager.Shared.DTOs.Responses.Team;

namespace FlowManager.Infrastructure.Services
{
    public class FlowService : IFlowService
    {
        private readonly IFlowRepository _flowRepository;
        private readonly IFormTemplateRepository _formTemplateRepository;
        private readonly IStepRepository _stepRepository;
        private readonly ITeamRepository _teamRepository;
        private readonly IRoleRepository _roleRepository;

        public FlowService(IFlowRepository flowRepository,
            IFormTemplateRepository formTemplateRepository,
            IStepRepository stepRepository,
            ITeamRepository teamRepository,
            IRoleRepository roleRepository)
        {
            _flowRepository = flowRepository;
            _formTemplateRepository = formTemplateRepository;
            _stepRepository = stepRepository;
            _teamRepository = teamRepository;
            _roleRepository = roleRepository;
        }

        public async Task<PagedResponseDto<FlowResponseDto>> GetAllFlowsQueriedAsync(QueriedFlowRequestDto payload)
        {
            QueryParams? parameters = payload.QueryParams?.ToQueryParams();
            (List<Flow> data, int totalCount) = await _flowRepository.GetAllFlowsQueriedAsync(payload.GlobalSearchTerm, parameters);

            return new PagedResponseDto<FlowResponseDto>
            {
                Data = data.Select(f => new FlowResponseDto
                {
                    Id = f.Id,
                    Name = f.Name,
                    Steps = f.Steps
                        .OrderBy(s => s.Order) // Order by Order field
                        .Select(s => new StepResponseDto
                        {
                            Id = s.Step.Id,
                            Name = s.Step.Name,
                        }).ToList(),
                    FormTemplateId = f.ActiveFormTemplateId,
                    ActiveFormTemplate = f.ActiveFormTemplate != null ? MapToFormTemplateResponseDto(f.ActiveFormTemplate) : null,
                    FormTemplates = f.FormTemplateFlows?.Select(formTemplateFlows => formTemplateFlows.FormTemplate).Select(MapToFormTemplateResponseDto).ToList(),
                    CreatedAt = f.CreatedAt,
                    UpdatedAt = f.UpdatedAt,
                    DeletedAt = f.DeletedAt
                }).ToList(),
                TotalCount = totalCount,
                PageSize = parameters?.PageSize ?? totalCount,
                Page = parameters?.Page ?? 1,
            };
        }

        public async Task<FlowResponseDto> GetFlowByIdAsync(Guid id)
        {
            Guid moderatorRoleId = (await _roleRepository.GetRoleByRolenameAsync("MODERATOR"))!.Id;
            Flow? flow = await _flowRepository.GetFlowByIdIncludeStepsAsync(id, moderatorRoleId);

            if (flow == null)
            {
                throw new EntryNotFoundException($"Flow with id {id} was not found.");
            }

            return new FlowResponseDto
            {
                Id = flow.Id,
                Name = flow.Name,
                Steps = flow.Steps
                    .OrderBy(s => s.Order) // Order by Order field
                    .Select(s => new StepResponseDto
                    {
                        Id = s.Step.Id,
                        Name = s.Step.Name,
                        Users = s.AssignedUsers?.Select(u => new Shared.DTOs.Responses.User.UserResponseDto
                        {
                            Id = u.UserId,
                        }).ToList(),
                        Teams = s.AssignedTeams?.Select(t => new Shared.DTOs.Responses.Team.TeamResponseDto
                        {
                            Id = t.TeamId,
                        }).ToList(),
                    }).ToList(),
                FormTemplateId = flow.ActiveFormTemplateId,
                ActiveFormTemplate = flow.ActiveFormTemplate != null ? MapToFormTemplateResponseDto(flow.ActiveFormTemplate) : null,
                FormTemplates = flow.FormTemplateFlows?.Select(formTemplateFlows => formTemplateFlows.FormTemplate).Select(MapToFormTemplateResponseDto).ToList(),
                CreatedAt = flow.CreatedAt,
                UpdatedAt = flow.UpdatedAt,
                DeletedAt = flow.DeletedAt
            };
        }

        public async Task<FlowResponseDto> CreateFlowAsync(PostFlowRequestDto payload)
        {
            Flow? existingFlow = await _flowRepository.GetFlowByNameAsync(payload.Name);
            if (existingFlow != null)
            {
                if (existingFlow.DeletedAt != null)
                {
                    throw new UniqueConstraintViolationException($"Flow with name {payload.Name} was previously deleted. Please choose a different name.");
                }
                else
                {
                    throw new UniqueConstraintViolationException($"Flow with name {payload.Name} already exists. Please choose a different name.");
                }
            }

            Flow flowToPost = new Flow
            {
                Name = payload.Name,
            };

            if (payload.FormTemplateId != null && payload.FormTemplateId != Guid.Empty)
            {
                FormTemplate? ft = await _formTemplateRepository.GetFormTemplateByIdAsync((Guid)payload.FormTemplateId);
                if (ft == null)
                {
                    throw new EntryNotFoundException($"Form template with id {payload.FormTemplateId} not found.");
                }

                flowToPost.FormTemplateFlows.Add(new FormTemplateFlow
                {
                    FormTemplateId = ft.Id,
                    FlowId = flowToPost.Id,
                });
            }

            if (payload.Steps != null && payload.Steps.Count > 0)
            {
                for (int i = 0; i < payload.Steps.Count; i++)
                {
                    var stepPayload = payload.Steps[i];

                    if ((await _stepRepository.GetStepByIdAsync(stepPayload.StepId)) == null)
                    {
                        throw new EntryNotFoundException($"Step with id {stepPayload.StepId} not found.");
                    }

                    var flowStep = new FlowStep
                    {
                        StepId = stepPayload.StepId,
                        FlowId = flowToPost.Id,
                        Order = i + 1,
                    };

                    flowStep.AssignedUsers = stepPayload.UserIds?.Select(uId => new FlowStepUser
                    {
                        FlowStepId = flowStep.Id,
                        UserId = uId,
                    }).ToList() ?? new List<FlowStepUser>();

                    foreach (var teamPayload in stepPayload.Teams ?? new List<PostFlowTeamRequestDto>())
                    {
                        var fullTeam = await _teamRepository.GetTeamWithModeratorsAsync(teamPayload.TeamId, (await _roleRepository.GetRoleByRolenameAsync("MODERATOR"))!.Id);

                        if (fullTeam?.Users.Count == (teamPayload.UserIds?.Count ?? 0))
                        {
                            flowStep.AssignedTeams.Add(new FlowStepTeam
                            {
                                FlowStepId = flowStep.Id,
                                TeamId = teamPayload.TeamId,
                            });
                        }
                        else
                        {
                            foreach (var userId in teamPayload.UserIds ?? new List<Guid>())
                            {
                                flowStep.AssignedUsers.Add(new FlowStepUser
                                {
                                    FlowStepId = flowStep.Id,
                                    UserId = userId,
                                });
                            }
                        }
                    }

                    flowToPost.Steps.Add(flowStep);
                }
            }

            await _flowRepository.CreateFlowAsync(flowToPost);

            return new FlowResponseDto
            {
                Id = flowToPost.Id,
                Name = flowToPost.Name,
                Steps = flowToPost.Steps
                    .OrderBy(s => s.Order) // Order by Order field
                    .Select(s => new StepResponseDto
                    {
                        Id = s.StepId,
                        Users = s.AssignedUsers?.Select(u => new Shared.DTOs.Responses.User.UserResponseDto
                        {
                            Id = u.UserId,
                        }).ToList(),
                        Teams = s.AssignedTeams?.Select(t => new Shared.DTOs.Responses.Team.TeamResponseDto
                        {
                            Id = t.TeamId,
                        }).ToList(),
                    }).ToList(),
                FormTemplateId = flowToPost.ActiveFormTemplateId,
                ActiveFormTemplate = flowToPost.ActiveFormTemplate != null ? MapToFormTemplateResponseDto(flowToPost.ActiveFormTemplate) : null,
                FormTemplates = flowToPost.FormTemplateFlows?.Select(formTemplateFlows => formTemplateFlows.FormTemplate).Select(MapToFormTemplateResponseDto).ToList(),
                CreatedAt = flowToPost.CreatedAt,
                UpdatedAt = flowToPost.UpdatedAt,
                DeletedAt = flowToPost.DeletedAt
            };
        }

        public async Task<FlowResponseDto> UpdateFlowAsync(Guid id, PatchFlowRequestDto payload)
        {
            Flow? flowToUpdate = await _flowRepository.GetFlowIncludeDeletedStepsByIdAsync(id);

            if (flowToUpdate == null)
            {
                throw new EntryNotFoundException($"Flow with id {id} was not found.");
            }

            if (!string.IsNullOrEmpty(payload.Name))
            {
                flowToUpdate.Name = payload.Name;
            }

            // Note: FormTemplateId is removed from PatchFlowRequestDto since Flow versioning handles this automatically

            if (payload.StepIds == null || !payload.StepIds.Any())
            {
                await _flowRepository.SaveChangesAsync();
                return new FlowResponseDto
                {
                    Id = flowToUpdate.Id,
                    Name = flowToUpdate.Name,
                    Steps = flowToUpdate.Steps
                        .OrderBy(s => s.Order) // Order by Order field
                        .Select(s => new StepResponseDto
                        {
                            Id = s.Step.Id,
                            Name = s.Step.Name,
                        }).ToList(),
                    FormTemplateId = flowToUpdate.ActiveFormTemplateId,
                    ActiveFormTemplate = flowToUpdate.ActiveFormTemplate != null ? MapToFormTemplateResponseDto(flowToUpdate.ActiveFormTemplate) : null,
                    FormTemplates = flowToUpdate.FormTemplateFlows?.Select(formTemplateFlows => formTemplateFlows.FormTemplate).Select(MapToFormTemplateResponseDto).ToList(),
                    CreatedAt = flowToUpdate.CreatedAt,
                    UpdatedAt = flowToUpdate.UpdatedAt,
                    DeletedAt = flowToUpdate.DeletedAt
                };
            }

            foreach (Guid stepId in payload.StepIds)
            {
                if (await _stepRepository.GetStepByIdAsync(stepId) == null)
                {
                    throw new EntryNotFoundException($"Step with id {stepId} was not found.");
                }
            }

            foreach (FlowStep step in flowToUpdate.Steps)
            {
                step.DeletedAt = DateTime.UtcNow;
            }

            // Get the next order number
            int nextOrder = 1;
            if (flowToUpdate.Steps.Any(s => s.DeletedAt == null))
            {
                nextOrder = flowToUpdate.Steps.Where(s => s.DeletedAt == null).Max(s => s.Order) + 1;
            }

            foreach (Guid stepId in payload.StepIds)
            {
                if (flowToUpdate.Steps.Any(s => s.StepId == stepId))
                {
                    FlowStep? existingStep = flowToUpdate.Steps.FirstOrDefault(fs => fs.StepId == stepId);
                    if (existingStep != null)
                    {
                        existingStep.DeletedAt = null;
                    }
                }
                else
                {
                    flowToUpdate.Steps.Add(new FlowStep
                    {
                        StepId = stepId,
                        FlowId = flowToUpdate.Id,
                        Order = nextOrder++ // Set order sequentially
                    });
                }
            }
            await _flowRepository.SaveChangesAsync();

            return new FlowResponseDto
            {
                Id = flowToUpdate.Id,
                Name = flowToUpdate.Name,
                Steps = flowToUpdate.Steps
                    .OrderBy(s => s.Order) // Order by Order field
                    .Select(s => new StepResponseDto
                    {
                        Id = s.Step.Id,
                        Name = s.Step.Name,
                    }).ToList(),
                FormTemplateId = flowToUpdate.ActiveFormTemplateId,
                ActiveFormTemplate = flowToUpdate.ActiveFormTemplate != null ? MapToFormTemplateResponseDto(flowToUpdate.ActiveFormTemplate) : null,
                FormTemplates = flowToUpdate.FormTemplateFlows?.Select(formTemplateFlows => formTemplateFlows.FormTemplate).Select(MapToFormTemplateResponseDto).ToList(),
                CreatedAt = flowToUpdate.CreatedAt,
                UpdatedAt = flowToUpdate.UpdatedAt,
                DeletedAt = flowToUpdate.DeletedAt
            };
        }

        public async Task<FlowResponseDto> DeleteFlowAsync(Guid id)
        {
            Flow? flowToDelete = await _flowRepository.GetFlowByIdAsync(id);
            if (flowToDelete == null)
            {
                throw new EntryNotFoundException($"Flow with id {id} was not found.");
            }

            flowToDelete.DeletedAt = DateTime.UtcNow;
            await _flowRepository.SaveChangesAsync();
            return new FlowResponseDto
            {
                Id = flowToDelete.Id,
                Name = flowToDelete.Name,
                Steps = flowToDelete.Steps
                    .OrderBy(s => s.Order) // Order by Order field
                    .Select(s => new StepResponseDto
                    {
                        Id = s.Step.Id,
                        Name = s.Step.Name,
                    }).ToList(),
                FormTemplateId = flowToDelete.ActiveFormTemplateId,
                ActiveFormTemplate = flowToDelete.ActiveFormTemplate != null ? MapToFormTemplateResponseDto(flowToDelete.ActiveFormTemplate) : null,
                FormTemplates = flowToDelete.FormTemplateFlows?.Select(formTemplateFlows => formTemplateFlows.FormTemplate).Select(MapToFormTemplateResponseDto).ToList(),
                CreatedAt = flowToDelete.CreatedAt,
                UpdatedAt = flowToDelete.UpdatedAt,
                DeletedAt = flowToDelete.DeletedAt
            };
        }

        public async Task<List<StepResponseDto>> GetStepsForFlowAsync(Guid flowId)
        {
            Flow? flow = await _flowRepository.GetFlowByIdAsync(flowId);

            if (flow == null)
            {
                throw new EntryNotFoundException($"Flow with id {flowId} was not found.");
            }

            return new List<StepResponseDto>(
                flow.Steps
                    .OrderBy(fs => fs.Order) // Order by Order field instead of CreatedAt
                    .Select(fs => new StepResponseDto
                    {
                        Id = fs.Step.Id,
                        Name = fs.Step.Name,
                        CreatedAt = fs.Step.CreatedAt,
                        UpdatedAt = fs.Step.UpdatedAt,
                        DeletedAt = fs.Step.DeletedAt
                    })
            );
        }

        public async Task<FlowResponseDto> GetFlowByIdIncludeStepsAsync(Guid flowId)
        {
            Guid moderatorId = (await _roleRepository.GetRoleByRolenameAsync("MODERATOR"))!.Id;

            Flow? flow = await _flowRepository.GetFlowByIdIncludeStepsAsync(flowId, moderatorId);
            if (flow == null)
            {
                throw new EntryNotFoundException($"Flow with id {flowId} was not found.");
            }

            flow.Steps = flow.Steps.OrderBy(fs => fs.Order).ToList();

            FlowResponseDto response = new FlowResponseDto
            {
                Id = flowId,
                Name = flow.Name,
                FlowSteps = flow.Steps.Select(fs => new FlowStepResponseDto
                {
                    Id = fs.Id,
                    StepId = fs.Step.Id,
                    StepName = fs.Step.Name,
                    Teams = fs.AssignedTeams.Select(assignedTeam => new FlowStepTeamResponseDto
                    {
                        FlowStepTeamId = assignedTeam.Id,
                        Team = new TeamResponseDto
                        {
                            Id = assignedTeam.TeamId,
                            Name = assignedTeam.Team.Name,
                            Users = assignedTeam.Team.Users.Select(u => new Shared.DTOs.Responses.User.UserResponseDto
                            {
                                Id = u.UserId,
                                Name = u.User.Name,
                                Email = u.User.Email,
                            }).ToList()
                        }
                    }).ToList()
                }).ToList(),
            };

            for (int i = 0; i < flow.Steps.Count; i++)
            {
                var flowStep = flow.Steps.ElementAt(i);

                var usersWithoutTeams = flowStep.AssignedUsers
                    .Where(assignedUser => assignedUser.User.Teams.Count == 0)
                    .ToList();

                response.FlowSteps[i].Users = usersWithoutTeams
                    .Select(assignedUser => new Shared.DTOs.Responses.User.FlowStepUserResponseDto
                    {
                        FlowStepUserId = assignedUser.Id,
                        User = new Shared.DTOs.Responses.User.UserResponseDto
                        {
                            Id = assignedUser.UserId,
                            Name = assignedUser.User.Name,
                            Email = assignedUser.User.Email,
                            Teams = assignedUser.User.Teams.Select(ut => new TeamResponseDto()
                            {
                                Id = ut.TeamId,
                                Name = ut.Team.Name
                            }).ToList(),
                        }
                    }).ToList();

                var usersWithTeams = flowStep.AssignedUsers
                    .Where(assignedUser => assignedUser.User.Teams.Count > 0)
                    .ToList();

                foreach (var assignedUser in usersWithTeams)
                {
                    foreach (var userTeam in assignedUser.User.Teams)
                    {
                        var teamInResponse = response.FlowSteps[i].Teams
                            .FirstOrDefault(t => t.Team.Id == userTeam.TeamId);

                        if (teamInResponse != null)
                        {
                            var userExists = teamInResponse.Team.Users
                                .Any(u => u.Id == assignedUser.UserId);


                            if (!userExists)
                            {
                                teamInResponse.Team.Users.Add(new Shared.DTOs.Responses.User.UserResponseDto
                                {
                                    Id = assignedUser.UserId,
                                    Name = assignedUser.User.Name,
                                    Email = assignedUser.User.Email,
                                });
                            }
                        }
                        else
                        {
                            var newTeam = new FlowStepTeamResponseDto
                            {
                                FlowStepTeamId = Guid.NewGuid(), 
                                Team = new TeamResponseDto
                                {
                                    Id = userTeam.TeamId,
                                    Name = userTeam.Team.Name,
                                    Users = new List<Shared.DTOs.Responses.User.UserResponseDto>
                                    {
                                        new Shared.DTOs.Responses.User.UserResponseDto
                                        {
                                            Id = assignedUser.UserId,
                                            Name = assignedUser.User.Name,
                                            Email = assignedUser.User.Email,
                                        }
                                    }
                                }
                            };

                            response.FlowSteps[i].Teams.Add(newTeam);
                        }
                    }
                }

            }

            return response;
        }

        private FormTemplateResponseDto MapToFormTemplateResponseDto(FormTemplate ft)
        {
            return new FormTemplateResponseDto
            {
                Id = ft.Id,
                Name = ft.Name,
                Content = ft.Content,
                FlowId = ft.ActiveFlowId,
                Components = ft.Components?.Where(ftc => ftc.DeletedAt == null).Select(ftc => new FormTemplateComponentResponseDto
                {
                    Id = ftc.ComponentId,
                }).ToList(),
                CreatedAt = ft.CreatedAt,
                UpdatedAt = ft.UpdatedAt,
                DeletedAt = ft.DeletedAt
            };
        }

        public async Task<FlowResponseDto> GetFlowByFormTemplateIdAsync(Guid formTemplateId)
        {
            Flow? activeFlowForFormTemplate = await _flowRepository.GetFlowByFormTemplateIdAsync(formTemplateId);

            if(activeFlowForFormTemplate == null)
            {
                throw new EntryNotFoundException($"Flow not found for form template {formTemplateId}.");
            }

            return new FlowResponseDto
            {
                Id = activeFlowForFormTemplate.Id,
                Name = activeFlowForFormTemplate.Name,
                FlowSteps = activeFlowForFormTemplate.Steps
                    .OrderBy(s => s.Order)
                    .Select(fs => new FlowStepResponseDto
                    {
                        FlowId = fs.FlowId,
                        StepId = fs.StepId,
                        StepName = fs.Step.Name,
                    }).ToList(),
            };
        }

        public async Task<bool> GetFlowNameUnicityAsync(string flowName)
        {
            Flow? existingFlow = await _flowRepository.GetFlowByNameAsync(flowName);

            return existingFlow == null;
        }
    }
}