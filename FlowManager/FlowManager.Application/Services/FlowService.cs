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
            (List<Flow> data, int totalCount) = await _flowRepository.GetAllFlowsQueriedAsync(payload.Name, parameters);

            return new PagedResponseDto<FlowResponseDto>
            {
                Data = data.Select(f => new FlowResponseDto
                {
                    Id = f.Id,
                    Name = f.Name,
                    Steps = f.Steps.Select(s => new StepResponseDto
                    {
                        Id = s.Step.Id,
                        Name = s.Step.Name,
                    }).ToList(),
                    FormTemplateId = f.FormTemplateId,
                    ActiveFormTemplate = f.ActiveFormTemplate != null ? MapToFormTemplateResponseDto(f.ActiveFormTemplate) : null,
                    FormTemplates = f.FormTemplates?.Select(MapToFormTemplateResponseDto).ToList(),
                    CreatedAt = f.CreatedAt,
                    UpdatedAt = f.UpdatedAt,
                    DeletedAt = f.DeletedAt
                }).ToList(),
                TotalCount = totalCount
            };
        }

        public async Task<FlowResponseDto> GetFlowByIdAsync(Guid id)
        {
            Flow? flow = await _flowRepository.GetFlowByIdAsync(id);

            if (flow == null)
            {
                throw new EntryNotFoundException($"Flow with id {id} was not found.");
            }

            return new FlowResponseDto
            {
                Id = flow.Id,
                Name = flow.Name,
                Steps = flow.Steps.Select(s => new StepResponseDto
                {
                    Id = s.Step.Id,
                    Name = s.Step.Name,
                }).ToList(),
                FormTemplateId = flow.FormTemplateId,
                ActiveFormTemplate = flow.ActiveFormTemplate != null ? MapToFormTemplateResponseDto(flow.ActiveFormTemplate) : null,
                FormTemplates = flow.FormTemplates?.Select(MapToFormTemplateResponseDto).ToList(),
                CreatedAt = flow.CreatedAt,
                UpdatedAt = flow.UpdatedAt,
                DeletedAt = flow.DeletedAt
            };
        }

        public async Task<FlowResponseDto> CreateFlowAsync(PostFlowRequestDto payload)
        {
            Flow flowToPost = new Flow
            {
                Name = payload.Name,
            };

            if(payload.FormTemplateId != null)
            {
                FormTemplate? ft = await _formTemplateRepository.GetFormTemplateByIdAsync((Guid)payload.FormTemplateId);
                if (ft == null)
                {
                    throw new EntryNotFoundException($"Form template with id {payload.FormTemplateId} not found.");
                }

                flowToPost.FormTemplates.Add(ft);
            }

            if (payload.Steps != null && payload.Steps.Count > 0)
            {
                foreach(PostFlowStepRequestDto step in payload.Steps)
                {
                    if ((await _stepRepository.GetStepByIdAsync(step.StepId)) == null)
                    { 
                        throw new EntryNotFoundException($"Step with id {payload.FormTemplateId} not found.");
                    }

                    flowToPost.Steps.Add(new FlowStep
                    {
                        StepId = step.StepId,
                        FlowId = flowToPost.Id,
                    });
                }

                foreach (var step in flowToPost.Steps)
                {
                    var stepPayload = payload.Steps.First(s => s.StepId == step.StepId);

                    step.AssignedUsers = stepPayload.UserIds.Select(uId => new FlowStepUser
                    {
                        FlowStepId = step.Id,
                        UserId = uId,
                    }).ToList();

                    foreach (var teamPayload in stepPayload.Teams)
                    {
                        var fullTeam = await _teamRepository.GetTeamWithModeratorsAsync(teamPayload.TeamId, (await _roleRepository.GetRoleByRolenameAsync("MODERATOR"))!.Id);

                        if (fullTeam.Users.Count == teamPayload.UserIds.Count)
                        {
                            step.AssignedTeams.Add(new FlowStepTeam
                            {
                                FlowStepId = step.Id,
                                TeamId = teamPayload.TeamId,
                            });
                        }
                        else
                        {
                            foreach (var userId in teamPayload.UserIds)
                            {
                                step.AssignedUsers.Add(new FlowStepUser
                                {
                                    FlowStepId = step.Id,
                                    UserId = userId,
                                });
                            }
                        }
                    }
                }
            }

            await _flowRepository.CreateFlowAsync(flowToPost);

            return new FlowResponseDto
            {
                Id = flowToPost.Id,
                Name = flowToPost.Name,
                Steps = flowToPost.Steps.Select(s => new StepResponseDto
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
                FormTemplateId = flowToPost.FormTemplateId,
                ActiveFormTemplate = flowToPost.ActiveFormTemplate != null ? MapToFormTemplateResponseDto(flowToPost.ActiveFormTemplate) : null,
                FormTemplates = flowToPost.FormTemplates?.Select(MapToFormTemplateResponseDto).ToList(),
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
                    Steps = flowToUpdate.Steps.Select(s => new StepResponseDto
                    {
                        Id = s.Step.Id,
                        Name = s.Step.Name,
                    }).ToList(),
                    FormTemplateId = flowToUpdate.FormTemplateId,
                    ActiveFormTemplate = flowToUpdate.ActiveFormTemplate != null ? MapToFormTemplateResponseDto(flowToUpdate.ActiveFormTemplate) : null,
                    FormTemplates = flowToUpdate.FormTemplates?.Select(MapToFormTemplateResponseDto).ToList(),
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
                        FlowId = flowToUpdate.Id
                    });
                }
            }
            await _flowRepository.SaveChangesAsync();

            return new FlowResponseDto
            {
                Id = flowToUpdate.Id,
                Name = flowToUpdate.Name,
                Steps = flowToUpdate.Steps.Select(s => new StepResponseDto
                {
                    Id = s.Step.Id,
                    Name = s.Step.Name,
                }).ToList(),
                FormTemplateId = flowToUpdate.FormTemplateId,
                ActiveFormTemplate = flowToUpdate.ActiveFormTemplate != null ? MapToFormTemplateResponseDto(flowToUpdate.ActiveFormTemplate) : null,
                FormTemplates = flowToUpdate.FormTemplates?.Select(MapToFormTemplateResponseDto).ToList(),
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
                Steps = flowToDelete.Steps.Select(s => new StepResponseDto
                {
                    Id = s.Step.Id,
                    Name = s.Step.Name,
                }).ToList(),
                FormTemplateId = flowToDelete.FormTemplateId,
                ActiveFormTemplate = flowToDelete.ActiveFormTemplate != null ? MapToFormTemplateResponseDto(flowToDelete.ActiveFormTemplate) : null,
                FormTemplates = flowToDelete.FormTemplates?.Select(MapToFormTemplateResponseDto).ToList(),
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
                flow.Steps.Select(fs => new StepResponseDto
                {
                    Id = fs.Step.Id,
                    Name = fs.Step.Name,
                    CreatedAt = fs.Step.CreatedAt,
                    UpdatedAt = fs.Step.UpdatedAt,
                    DeletedAt = fs.Step.DeletedAt
                })
            );
        }

        private FormTemplateResponseDto MapToFormTemplateResponseDto(FormTemplate ft)
        {
            return new FormTemplateResponseDto
            {
                Id = ft.Id,
                Name = ft.Name,
                Content = ft.Content,
                FlowId = ft.FlowId,
                Components = ft.Components?.Where(ftc => ftc.DeletedAt == null).Select(ftc => new FormTemplateComponentResponseDto
                {
                    Id = ftc.ComponentId,
                }).ToList(),
                CreatedAt = ft.CreatedAt,
                UpdatedAt = ft.UpdatedAt,
                DeletedAt = ft.DeletedAt
            };
        }
    }
}