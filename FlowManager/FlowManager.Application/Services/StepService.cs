using FlowManager.Application.DTOs.Requests.Step;
using FlowManager.Application.DTOs.Responses;
using FlowManager.Application.DTOs.Responses.Step;
using FlowManager.Application.Interfaces;
using FlowManager.Domain.Dtos;
using FlowManager.Domain.Entities;
using FlowManager.Domain.Exceptions;
using FlowManager.Domain.IRepositories;

namespace FlowManager.Application.Services
{
    public class StepService : IStepService
    {
        private readonly IStepRepository _stepRepository;
        private readonly IUserRepository _userRepository;
        private readonly IFlowRepository _flowRepository;

        public StepService(IStepRepository stepRepository, IUserRepository userRepository, IFlowRepository flowRepository) 
        {
            _stepRepository = stepRepository;
            _userRepository = userRepository;
            _flowRepository = flowRepository;
        }

        public async Task<List<StepResponseDto>> GetStepsAsync()
        {
            var steps = await _stepRepository.GetStepsAsync();

            return steps.Select(s => new StepResponseDto
            {
                Id = s.Id,
                Name = s.Name,
                UserIds = s.Users.Select(u => u.Id).ToList(),
                FlowIds = s.FlowSteps.Select(fs => fs.FlowId).ToList(),
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt,
                DeletedAt = s.DeletedAt
            }).ToList();
        }

        public async Task<StepResponseDto> GetStepAsync(Guid id)
        {
            Step? step = await _stepRepository.GetStepByIdAsync(id);

            if(step == null)
            {
                throw new EntryNotFoundException($"Step with id {id} was not found.");
            }

            return new StepResponseDto
            {
                Id = step.Id,
                Name = step.Name,
                UserIds = step.Users.Select(u => u.Id).ToList(),
                FlowIds = step.FlowSteps.Select(fs => fs.FlowId).ToList(),
                CreatedAt = step.CreatedAt,
                UpdatedAt = step.UpdatedAt,
                DeletedAt = step.DeletedAt
            };
        }

        public async Task<StepResponseDto> PostStepAsync(PostStepRequestDto payload)
        {
            Step stepToPost = new Step
            {
                Name = payload.Name
            };

            foreach(Guid userId in payload.UserIds)
            {
                User? user = await _userRepository.GetUserByIdAsync(userId);
                if(user == null)
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
                UserIds = stepToPost.Users.Select(u => u.Id).ToList(),
                FlowIds = stepToPost.FlowSteps.Select(fs => fs.FlowId).ToList(),
                CreatedAt = stepToPost.CreatedAt,
                UpdatedAt = stepToPost.UpdatedAt,
                DeletedAt = stepToPost.DeletedAt
            };
        }

        public async Task<StepResponseDto> PatchStepAsync(Guid id, PatchStepRequestDto payload)
        {
            Step? stepToPatch = await _stepRepository.GetStepByIdAsync(id);

            if(stepToPatch == null)
            {
                throw new EntryNotFoundException($"Step with id {id} was not found.");
            }

            if(!string.IsNullOrEmpty(payload.Name))
            {
                stepToPatch.Name = payload.Name;
            }

            await _stepRepository.SaveChangesAsync();

            return new StepResponseDto
            {
                Id = stepToPatch.Id,
                Name = stepToPatch.Name,
                UserIds = stepToPatch.Users.Select(u => u.Id).ToList(),
                FlowIds = stepToPatch.FlowSteps.Select(fs => fs.FlowId).ToList(),
                CreatedAt = stepToPatch.CreatedAt,
                UpdatedAt = stepToPatch.UpdatedAt,
                DeletedAt = stepToPatch.DeletedAt
            };
        }

        public async Task<StepResponseDto> DeleteStepAsync(Guid id)
        {
            Step? step = await _stepRepository.GetStepByIdAsync(id);

            if(step == null)
            {
                throw new EntryNotFoundException($"Step with id {id} was not found.");
            }

            await _stepRepository.DeleteStepAsync(step);

            return new StepResponseDto
            {
                Id = step.Id,
                Name = step.Name,
                UserIds = step.Users.Select(u => u.Id).ToList(),
                FlowIds = step.FlowSteps.Select(fs => fs.FlowId).ToList(),
                CreatedAt = step.CreatedAt,
                UpdatedAt = step.UpdatedAt,
                DeletedAt = DateTime.UtcNow
            };
        }

        public async Task<StepResponseDto> AssignUserToStepAsync(Guid id, Guid userId)
        {
            User? user = await _userRepository.GetUserByIdAsync(userId);

            if(user == null)
            {
                throw new EntryNotFoundException($"User with id {userId} was not found.");
            }

            Step? step = await _stepRepository.GetStepByIdAsync(id);

            if(step == null)
            {
                throw new EntryNotFoundException($"Step with id {id} was not found.");
            }

            if (step.Users.FirstOrDefault(user) != null)
            {
                throw new UniqueConstraintViolationException($"Relationship between user id {user.Id} and step id {step.Id} already exists.");
            }

            step.Users.Add(user);

            await _stepRepository.SaveChangesAsync();

            return new StepResponseDto
            {
                Id = step.Id,
                Name = step.Name,
                UserIds = step.Users.Select(u => u.Id).ToList(),
                FlowIds = step.FlowSteps.Select(fs => fs.FlowId).ToList(),
                CreatedAt = step.CreatedAt,
                UpdatedAt = step.UpdatedAt,
                DeletedAt = step.DeletedAt
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

            if(user.StepId.HasValue && user.StepId != step.Id)
            {
                throw new EntryNotFoundException($"Relationship between user id {user.Id} and step id {step.Id} does not exist.");
            }

            if (user.StepId == null)
            {
                throw new UniqueConstraintViolationException($"Relationship between user id {user.Id} and step id {step.Id} already deleted.");
            }

            user.StepId = null;

            await _userRepository.SaveChangesAsync();

            return new StepResponseDto
            {
                Id = step.Id,
                Name = step.Name,
                UserIds = step.Users.Select(u => u.Id).ToList(),
                FlowIds = step.FlowSteps.Select(fs => fs.FlowId).ToList(),
                CreatedAt = step.CreatedAt,
                UpdatedAt = step.UpdatedAt,
                DeletedAt = step.DeletedAt
            };
        }

        public async Task<StepResponseDto> AddStepToFlowAsync(Guid stepId, Guid flowId)
        {
            var step = await _stepRepository.GetStepByIdAsync(stepId);
            if (step == null)
            { 
                throw new EntryNotFoundException($"Step {stepId} not found.");
            }

            var flow = await _flowRepository.GetFlowByIdAsync(flowId);
            if (flow == null)
            { 
                throw new EntryNotFoundException($"Flow {flowId} not found."); 
            }

            if (step.FlowSteps.Any(fs => fs.FlowId == flowId))
            { 
                throw new UniqueConstraintViolationException($"Step {stepId} is already part of flow {flowId}."); 
            }

            step.FlowSteps.Add(new FlowStep { StepId = stepId, FlowId = flowId });
            await _stepRepository.SaveChangesAsync();

            return new StepResponseDto
            {
                Id = step.Id,
                Name = step.Name,
                UserIds = step.Users.Select(u => u.Id).ToList(),
                FlowIds = step.FlowSteps.Select(fs => fs.FlowId).ToList(),
                CreatedAt = step.CreatedAt,
                UpdatedAt = step.UpdatedAt,
                DeletedAt = step.DeletedAt
            };
        }

        public async Task<StepResponseDto> RemoveStepFromFlowAsync(Guid stepId, Guid flowId)
        {
            var step = await _stepRepository.GetStepByIdAsync(stepId);
            if (step == null)
            {
                throw new EntryNotFoundException($"Step {stepId} not found.");
            }

            var flow = await _flowRepository.GetFlowByIdAsync(flowId);
            if (flow == null)
            {
                throw new EntryNotFoundException($"Flow {flowId} not found.");
            }

            var flowStep = step.FlowSteps.FirstOrDefault(fs => fs.FlowId == flowId && fs.DeletedAt == null);
            if (flowStep == null)
            {
                throw new InvalidOperationException($"Step {stepId} is not part of flow {flowId}.");
            }

            flowStep.DeletedAt = DateTime.UtcNow;

            await _stepRepository.SaveChangesAsync();

            return new StepResponseDto
            {
                Id = step.Id,
                Name = step.Name,
                UserIds = step.Users.Select(u => u.Id).ToList(),
                FlowIds = step.FlowSteps.Where(fs => fs.DeletedAt == null).Select(fs => fs.FlowId).ToList(),
                CreatedAt = step.CreatedAt,
                UpdatedAt = step.UpdatedAt,
                DeletedAt = step.DeletedAt
            };
        }

        public async Task<StepResponseDto> RestoreStepToFlowAsync(Guid stepId, Guid flowId)
        {
            var step = await _stepRepository.GetStepByIdAsync(stepId);
            if (step == null)
            {
                throw new EntryNotFoundException($"Step {stepId} not found.");
            }

            var flow = await _flowRepository.GetFlowByIdAsync(flowId);
            if (flow == null)
            {
                throw new EntryNotFoundException($"Flow {flowId} not found.");
            }

            if (step.FlowSteps.Any(fs => fs.FlowId == flowId && fs.DeletedAt == null))
            {
                throw new InvalidOperationException($"Step {stepId} is already part of flow {flowId}.");
            }

            var deletedFlowStep = step.FlowSteps.FirstOrDefault(fs => fs.FlowId == flowId && fs.DeletedAt != null);
            if (deletedFlowStep == null)
            {
                throw new InvalidOperationException($"No deleted relationship found between step {stepId} and flow {flowId}.");
            }

            deletedFlowStep.DeletedAt = null;

            await _stepRepository.SaveChangesAsync();

            return new StepResponseDto
            {
                Id = step.Id,
                Name = step.Name,
                UserIds = step.Users.Select(u => u.Id).ToList(),
                FlowIds = step.FlowSteps.Where(fs => fs.DeletedAt == null).Select(fs => fs.FlowId).ToList(),
                CreatedAt = step.CreatedAt,
                UpdatedAt = step.UpdatedAt,
                DeletedAt = step.DeletedAt
            };
        }

        public async Task<PagedResponseDto<StepResponseDto>> GetAllStepsQueriedAsync(QueriedStepRequestDto payload)
        {
            QueryParams? parameters = payload.QueryParams?.ToQueryParams();

            (List<Step> data, int totalCount) = await _stepRepository.GetAllStepsQueriedAsync(payload.Name, parameters);

            return new PagedResponseDto<StepResponseDto>
            {
                Data = data.Select(s => new StepResponseDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    UserIds = s.Users.Select(u => u.Id).ToList(),
                    FlowIds = s.FlowSteps.Select(fs => fs.FlowId).ToList(),
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt,
                    DeletedAt = s.DeletedAt
                }).ToList(),
                TotalCount = totalCount,
                Page = parameters?.Page ?? 1,
                PageSize = parameters?.PageSize ?? totalCount,
            };
        }
    }
}
