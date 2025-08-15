using FlowManager.Application.DTOs.Requests.Flow;
using FlowManager.Application.DTOs.Responses;
using FlowManager.Application.DTOs.Responses.Flow;
using FlowManager.Application.DTOs.Responses.Step;
using FlowManager.Application.Interfaces;
using FlowManager.Domain.Dtos;
using FlowManager.Domain.Entities;
using FlowManager.Domain.Exceptions;
using FlowManager.Domain.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace FlowManager.Infrastructure.Services
{
    public class FlowService : IFlowService
    {
        private readonly IFlowRepository _flowRepository;
        private readonly IFormTemplateRepository _formTemplateRepository;
        private readonly IStepRepository _stepRepository;

        public FlowService(IFlowRepository flowRepository, IFormTemplateRepository formTemplateRepository,IStepRepository stepRepository)
        {
            _flowRepository = flowRepository;   
            _formTemplateRepository = formTemplateRepository;
            _stepRepository = stepRepository;
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
                    StepIds = f.Steps.Select(s => s.StepId).ToList(),
                    FormTemplateId = f.FormTemplateId,
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

            if(flow == null)
            {
                throw new EntryNotFoundException($"Flow with id {id} was not found.");
            }

            return new FlowResponseDto
            {
                Id = flow.Id,
                Name = flow.Name,
                StepIds = flow.Steps.Select(s => s.StepId).ToList(),
                FormTemplateId = flow.FormTemplateId,
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
                FormTemplateId = payload.FormTemplateId,
            };

            flowToPost.Steps = payload.StepIds.Select(stepId => new FlowStep
            {
                StepId = stepId,
                FlowId = flowToPost.Id
            }).ToList();

            await _flowRepository.CreateFlowAsync(flowToPost);
            return new FlowResponseDto
            {
                Id = flowToPost.Id,
                Name = flowToPost.Name,
                StepIds = flowToPost.Steps.Select(s => s.StepId).ToList(),
                FormTemplateId = flowToPost.FormTemplateId,
                CreatedAt = flowToPost.CreatedAt,
                UpdatedAt = flowToPost.UpdatedAt,
                DeletedAt = flowToPost.DeletedAt
            };
        }

        public async Task<FlowResponseDto> UpdateFlowAsync(Guid id, PatchFlowRequestDto payload)
        {
            Flow? flowToUpdate = await _flowRepository.GetFlowIncludeDeletedStepsByIdAsync(id);

            if(flowToUpdate == null)
            {
                throw new EntryNotFoundException($"Flow with id {id} was not found.");
            }

            if(!string.IsNullOrEmpty(payload.Name))
            {
                flowToUpdate.Name = payload.Name;
            }

            if(payload.FormTemplateId.HasValue && 
                await _formTemplateRepository.GetFormTemplateByIdAsync(payload.FormTemplateId.Value) != null)
            {
                flowToUpdate.FormTemplateId = payload.FormTemplateId.Value;
            }

            if (payload.StepIds == null || !payload.StepIds.Any())
            {
                await _flowRepository.SaveChangesAsync();
                return new FlowResponseDto
                {
                    Id = flowToUpdate.Id,
                    Name = flowToUpdate.Name,
                    StepIds = flowToUpdate.Steps.Select(s => s.StepId).ToList(),
                    FormTemplateId = flowToUpdate.FormTemplateId,
                    CreatedAt = flowToUpdate.CreatedAt,
                    UpdatedAt = flowToUpdate.UpdatedAt,
                    DeletedAt = flowToUpdate.DeletedAt
                };
            }

            foreach (Guid stepId in payload.StepIds)
            {
                if(await _stepRepository.GetStepByIdAsync(stepId) == null)
                {
                    throw new EntryNotFoundException($"Step with id {stepId} was not found.");
                }
            }

            foreach(FlowStep step in flowToUpdate.Steps)
            {
                step.DeletedAt = DateTime.UtcNow;
            }

            foreach(Guid stepId in payload.StepIds)
            {
                if(flowToUpdate.Steps.Any(s => s.StepId == stepId))
                {
                    FlowStep? existingStep = flowToUpdate.Steps.FirstOrDefault(fs => fs.StepId == stepId);
                    if(existingStep != null)
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
                StepIds = flowToUpdate.Steps.Select(s => s.StepId).ToList(),
                FormTemplateId = flowToUpdate.FormTemplateId,
                CreatedAt = flowToUpdate.CreatedAt,
                UpdatedAt = flowToUpdate.UpdatedAt,
                DeletedAt = flowToUpdate.DeletedAt
            };
        }

        public async Task<FlowResponseDto> DeleteFlowAsync(Guid id)
        {
            Flow? flowToDelete = await _flowRepository.GetFlowByIdAsync(id);
            if(flowToDelete == null)
            {
                throw new EntryNotFoundException($"Flow with id {id} was not found.");
            }

            flowToDelete.DeletedAt = DateTime.UtcNow;
            await _flowRepository.SaveChangesAsync();
            return new FlowResponseDto
            {
                Id = flowToDelete.Id,
                Name = flowToDelete.Name,
                StepIds = flowToDelete.Steps.Select(s => s.StepId).ToList(),
                FormTemplateId = flowToDelete.FormTemplateId,
                CreatedAt = flowToDelete.CreatedAt,
                UpdatedAt = flowToDelete.UpdatedAt,
                DeletedAt = flowToDelete.DeletedAt
            };
        }

        public async Task<List<StepResponseDto>> GetStepsForFlowAsync(Guid flowId)
        {
            Flow? flow = await _flowRepository.GetFlowByIdAsync(flowId);

            if(flow == null)
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
    }
}
