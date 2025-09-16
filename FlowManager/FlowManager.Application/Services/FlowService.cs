using FlowManager.Application.Interfaces;
using FlowManager.Application.Utils;
using FlowManager.Domain.Dtos;
using FlowManager.Domain.Entities;
using FlowManager.Domain.Exceptions;
using FlowManager.Domain.IRepositories;
using FlowManager.Shared.DTOs.Requests.Flow;
using FlowManager.Shared.DTOs.Requests.FlowStep;
using FlowManager.Shared.DTOs.Responses;
using FlowManager.Shared.DTOs.Responses.Flow;
using FlowManager.Shared.DTOs.Responses.FlowStep;
using FlowManager.Shared.DTOs.Responses.FlowStepItem;
using FlowManager.Shared.DTOs.Responses.FlowStepItemTeam;
using FlowManager.Shared.DTOs.Responses.FlowStepItemUser;
using FlowManager.Shared.DTOs.Responses.FormTemplate;
using FlowManager.Shared.DTOs.Responses.FormTemplateComponent;
using FlowManager.Shared.DTOs.Responses.Step;
using FlowManager.Shared.DTOs.Responses.Team;
using Microsoft.EntityFrameworkCore;

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
                Data = data.Select(flow => new FlowResponseDto
                {
                    Id = flow.Id,
                    Name = flow.Name,
                    FlowSteps = flow.Steps
                        .OrderBy(flowStep => flowStep.Order)
                        .Select(flowStep => new FlowStepResponseDto
                        {
                            Id = flowStep.Id,
                            FlowId = flowStep.FlowId,
                            FlowStepItems = flowStep.FlowStepItems.Select(flowStepItem => new Shared.DTOs.Responses.FlowStepItem.FlowStepItemResponseDto
                            {
                                FlowStepId = flowStepItem.FlowStepId,
                                StepId = flowStepItem.StepId,
                                Step = new StepResponseDto
                                {
                                    StepId = flowStepItem.StepId,
                                    StepName = flowStepItem.Step.Name
                                },
                                Order = flowStepItem.Order
                            }).OrderBy(flowStepItem => flowStepItem.Order).ToList(),
                        }).ToList(),
                    FormTemplateId = flow.ActiveFormTemplateId,
                    ActiveFormTemplate = flow.ActiveFormTemplate != null ? MapToFormTemplateResponseDto(flow.ActiveFormTemplate) : null,
                    FormTemplates = flow.FormTemplateFlows?.Select(formTemplateFlows => formTemplateFlows.FormTemplate).Select(MapToFormTemplateResponseDto).ToList(),
                    CreatedAt = flow.CreatedAt,
                    UpdatedAt = flow.UpdatedAt,
                    DeletedAt = flow.DeletedAt
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
                FlowSteps = flow.Steps
                    .OrderBy(step => step.Order) // Order by Order field
                    .Select(flowStep => new FlowStepResponseDto
                    {
                        Id = flowStep.Id,
                        FlowId = flowStep.FlowId,
                        FlowStepItems = flowStep.FlowStepItems.Select(flowStepItem => new Shared.DTOs.Responses.FlowStepItem.FlowStepItemResponseDto
                        {
                            FlowStepId = flowStepItem.FlowStepId,
                            StepId = flowStepItem.StepId,
                            Step = new StepResponseDto
                            {
                                StepId = flowStepItem.StepId,
                                StepName = flowStepItem.Step.Name,
                                Users = flowStepItem.AssignedUsers?.Select(u => new Shared.DTOs.Responses.User.UserResponseDto
                                {
                                    Id = u.UserId,
                                }).ToList(),
                                Teams = flowStepItem.AssignedTeams?.Select(t => new Shared.DTOs.Responses.Team.TeamResponseDto
                                {
                                    Id = t.TeamId,
                                }).ToList(),
                            },
                            Order = flowStepItem.Order
                        }).OrderBy(flowStepItem => flowStepItem.Order).ToList(),
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

            if (payload.FlowSteps != null && payload.FlowSteps.Count > 0)
            {
                for (int i = 0; i < payload.FlowSteps.Count; i++)
                {
                    var stepPayload = payload.FlowSteps[i];

                    foreach (var step in stepPayload.FlowStepItems)
                    { 
                        if ((await _stepRepository.GetStepByIdAsync(step.StepId)) == null)
                        {
                            throw new EntryNotFoundException($"Step with id {step.StepId} not found.");
                        } 
                    }

                    var flowStep = new FlowStep
                    {
                        FlowId = flowToPost.Id,
                        Order = i + 1,
                    };

                    var flowStepItems = new List<FlowStepItem>();

                    for (int index = 0; index < stepPayload.FlowStepItems.Count; index++)
                    {
                        var flowStepItem = stepPayload.FlowStepItems[index];

                        var newFlowStepItem = new FlowStepItem
                        {
                            FlowStepId = flowStepItem.FlowStepId,
                            StepId = flowStepItem.StepId,
                            Order = index + 1,
                        };

                        newFlowStepItem.AssignedUsers = flowStepItem.AssignedUsersIds.Select(uId => new FlowStepItemUser
                        {
                            FlowStepItemId = newFlowStepItem.Id,
                            UserId = uId,
                        }).ToList();

                        foreach (var teamPayload in flowStepItem.AssignedTeams)
                        {
                            var fullTeam = await _teamRepository.GetTeamWithModeratorsAsync(teamPayload.TeamId, (await _roleRepository.GetRoleByRolenameAsync("MODERATOR"))!.Id);
                            if (fullTeam?.Users.Count == (teamPayload.UserIds?.Count ?? 0))
                            {
                                newFlowStepItem.AssignedTeams.Add(new FlowStepItemTeam
                                {
                                    FlowStepItemId = newFlowStepItem.Id,
                                    TeamId = teamPayload.TeamId,
                                });
                            }
                            else
                            {
                                foreach (var userId in teamPayload.UserIds ?? new List<Guid>())
                                {
                                    newFlowStepItem.AssignedUsers.Add(new FlowStepItemUser
                                    {
                                        FlowStepItemId = newFlowStepItem.Id,
                                        UserId = userId,
                                    });
                                }
                            }
                        }

                        flowStepItems.Add(newFlowStepItem);
                    }

                    flowStep.FlowStepItems = flowStepItems;

                    flowToPost.Steps.Add(flowStep);
                }
            }

            await _flowRepository.CreateFlowAsync(flowToPost);

            return new FlowResponseDto
            {
                Id = flowToPost.Id,
                Name = flowToPost.Name,
                FlowSteps = flowToPost.Steps
                    .OrderBy(s => s.Order) // Order by Order field
                    .Select(flowStep => new FlowStepResponseDto
                    {
                        Id = flowStep.Id,
                        FlowId = flowStep.FlowId,
                        FlowStepItems = flowStep.FlowStepItems.Select(flowStepItem => new Shared.DTOs.Responses.FlowStepItem.FlowStepItemResponseDto
                        {
                            FlowStepId = flowStepItem.FlowStepId,
                            StepId = flowStepItem.StepId,
                            Step = new StepResponseDto
                            {
                                StepId = flowStepItem.StepId,
                                StepName = flowStepItem.Step.Name,
                                Users = flowStepItem.AssignedUsers?.Select(u => new Shared.DTOs.Responses.User.UserResponseDto
                                {
                                    Id = u.UserId,
                                }).ToList(),
                                Teams = flowStepItem.AssignedTeams?.Select(t => new Shared.DTOs.Responses.Team.TeamResponseDto
                                {
                                    Id = t.TeamId,
                                }).ToList(),
                            },
                            Order = flowStepItem.Order
                        }).OrderBy(flowStepItem => flowStepItem.Order).ToList(),
                    }).ToList(),
                FormTemplateId = flowToPost.ActiveFormTemplateId,
                ActiveFormTemplate = flowToPost.ActiveFormTemplate != null ? MapToFormTemplateResponseDto(flowToPost.ActiveFormTemplate) : null,
                FormTemplates = flowToPost.FormTemplateFlows?.Select(formTemplateFlows => formTemplateFlows.FormTemplate).Select(MapToFormTemplateResponseDto).ToList(),
                CreatedAt = flowToPost.CreatedAt,
                UpdatedAt = flowToPost.UpdatedAt,
                DeletedAt = flowToPost.DeletedAt
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
                FlowSteps = flowToDelete.Steps
                    .OrderBy(s => s.Order) // Order by Order field
                     .Select(flowStep => new FlowStepResponseDto
                     {
                         Id = flowStep.Id,
                         FlowId = flowStep.FlowId,
                         FlowStepItems = flowStep.FlowStepItems.Select(flowStepItem => new Shared.DTOs.Responses.FlowStepItem.FlowStepItemResponseDto
                         {
                             FlowStepId = flowStepItem.FlowStepId,
                             StepId = flowStepItem.StepId,
                             Step = new StepResponseDto
                             {
                                 StepId = flowStepItem.StepId,
                                 StepName = flowStepItem.Step.Name
                             },
                             Order = flowStepItem.Order
                         }).OrderBy(flowStepItem => flowStepItem.Order).ToList(),
                     }).ToList(),
                FormTemplateId = flowToDelete.ActiveFormTemplateId,
                ActiveFormTemplate = flowToDelete.ActiveFormTemplate != null ? MapToFormTemplateResponseDto(flowToDelete.ActiveFormTemplate) : null,
                FormTemplates = flowToDelete.FormTemplateFlows?.Select(formTemplateFlows => formTemplateFlows.FormTemplate).Select(MapToFormTemplateResponseDto).ToList(),
                CreatedAt = flowToDelete.CreatedAt,
                UpdatedAt = flowToDelete.UpdatedAt,
                DeletedAt = flowToDelete.DeletedAt
            };
        }

        public async Task<List<FlowResponseDto>> GetStepsForFlowAsync(Guid flowId)
        {
            Flow? flow = await _flowRepository.GetFlowByIdAsync(flowId);

            if (flow == null)
            {
                throw new EntryNotFoundException($"Flow with id {flowId} was not found.");
            }

            return new List<FlowResponseDto>(
                flow.Steps
                    .OrderBy(fs => fs.Order)
                     .Select(flowStep => new FlowResponseDto
                     {
                         Id = flow.Id,
                         Name = flow.Name,
                         FlowSteps = flow.Steps
                                            .OrderBy(flowStep => flowStep.Order)
                                            .Select(flowStep => new FlowStepResponseDto
                                            {
                                                Id = flowStep.Id,
                                                FlowId = flowStep.FlowId,
                                                FlowStepItems = flowStep.FlowStepItems.Select(flowStepItem => new Shared.DTOs.Responses.FlowStepItem.FlowStepItemResponseDto
                                                {
                                                    FlowStepId = flowStepItem.FlowStepId,
                                                    StepId = flowStepItem.StepId,
                                                    Step = new StepResponseDto
                                                    {
                                                        StepId = flowStepItem.StepId,
                                                        StepName = flowStepItem.Step.Name
                                                    },
                                                    Order = flowStepItem.Order
                                                }).OrderBy(flowStepItem => flowStepItem.Order).ToList(),
                                            }).ToList(),
                         FormTemplateId = flow.ActiveFormTemplateId,
                         ActiveFormTemplate = flow.ActiveFormTemplate != null ? MapToFormTemplateResponseDto(flow.ActiveFormTemplate) : null,
                         FormTemplates = flow.FormTemplateFlows?.Select(formTemplateFlows => formTemplateFlows.FormTemplate).Select(MapToFormTemplateResponseDto).ToList(),
                         CreatedAt = flow.CreatedAt,
                         UpdatedAt = flow.UpdatedAt,
                         DeletedAt = flow.DeletedAt
                     }).ToList());
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
                FlowSteps = flow.Steps.Select(flowStep => new FlowStepResponseDto
                {
                    Id = flowStep.Id,
                    FlowId = flowStep.FlowId,
                    FlowStepItems = flowStep.FlowStepItems.Select(flowStepItem => new Shared.DTOs.Responses.FlowStepItem.FlowStepItemResponseDto
                    {
                        FlowStepId = flowStepItem.FlowStepId,
                        StepId = flowStepItem.StepId,
                        Step = new StepResponseDto
                        {
                            StepId = flowStepItem.StepId,
                            StepName = flowStepItem.Step.Name,
                        },
                        AssignedTeams = flowStepItem.AssignedTeams?.Select(assignedTeam => new FlowStepItemTeamResponseDto
                        {
                            FlowStepItemId = flowStepItem.Id,
                            TeamId = assignedTeam.TeamId,
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
                        }).ToList(),
                        Order = flowStepItem.Order
                    }).OrderBy(flowStepItem => flowStepItem.Order).ToList(),
                }).ToList()
            };

            for (int i = 0; i < flow.Steps.Count; i++)
            {
                var flowStep = flow.Steps.ElementAt(i);

                for (int j = 0; j < flowStep.FlowStepItems.Count; j++)
                {
                    var usersWithoutTeams = flowStep.FlowStepItems[j].AssignedUsers
                        .Where(assignedUser => assignedUser.User.Teams.Count == 0)
                        .ToList();

                    response.FlowSteps[i].FlowStepItems[j].AssignedUsers = usersWithoutTeams.Select(assignedUser => new FlowStepItemUserResponseDto
                    {
                        FlowStepItemId = assignedUser.FlowStepItemId,
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

                    var usersWithTeams = flowStep.FlowStepItems[j].AssignedUsers
                        .Where(assignedUser => assignedUser.User.Teams.Count > 0)
                        .ToList();

                    foreach (var assignedUser in usersWithTeams)
                    {
                        foreach (var userTeam in assignedUser.User.Teams)
                        {
                            var teamInResponse = response.FlowSteps[i].FlowStepItems[j].AssignedTeams
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
                                var newTeam = new FlowStepItemTeamResponseDto
                                {
                                    FlowStepItemId = Guid.NewGuid(),
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

                                response.FlowSteps[i].FlowStepItems[j].AssignedTeams.Add(newTeam);
                            }
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
                     .Select(flowStep => new FlowStepResponseDto
                     {
                         Id = flowStep.Id,
                         FlowId = flowStep.FlowId,
                         FlowStepItems = flowStep.FlowStepItems.Select(flowStepItem => new Shared.DTOs.Responses.FlowStepItem.FlowStepItemResponseDto
                         {
                             FlowStepId = flowStepItem.FlowStepId,
                             StepId = flowStepItem.StepId,
                             Step = new StepResponseDto
                             {
                                 StepId = flowStepItem.StepId,
                                 StepName = flowStepItem.Step.Name
                             },
                             Order = flowStepItem.Order
                         }).OrderBy(flowStepItem => flowStepItem.Order).ToList(),
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