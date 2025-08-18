using FlowManager.Application.Interfaces;
using FlowManager.Domain.Dtos;
using FlowManager.Domain.Entities;
using FlowManager.Domain.Exceptions;
using FlowManager.Domain.IRepositories;
using FlowManager.Shared.DTOs.Requests.Step;
using FlowManager.Shared.DTOs.Responses;
using FlowManager.Shared.DTOs.Responses.Step;
using FlowManager.Application.Utils;

namespace FlowManager.Application.Services
{
    public class StepService : IStepService
    {
        private readonly IStepRepository _stepRepository;
        private readonly IUserRepository _userRepository;
        private readonly IFlowRepository _flowRepository;
        private readonly ITeamRepository _teamRepository;

        public StepService(
            IStepRepository stepRepository,
            IUserRepository userRepository,
            IFlowRepository flowRepository,
            ITeamRepository teamRepository)
        {
            _stepRepository = stepRepository;
            _userRepository = userRepository;
            _flowRepository = flowRepository;
            _teamRepository = teamRepository;
        }

        // Helper method pentru mapping
        private StepResponseDto MapToStepResponseDto(Step step)
        {
            return new StepResponseDto
            {
                Id = step.Id,
                Name = step.Name,
                UserIds = step.Users?.Where(su => su.DeletedAt == null).Select(su => su.UserId).ToList() ?? new List<Guid>(),
                TeamIds = step.Teams?.Where(st => st.DeletedAt == null).Select(st => st.TeamId).ToList() ?? new List<Guid>(),
                FlowIds = step.FlowSteps?.Where(fs => fs.DeletedAt == null).Select(fs => fs.FlowId).ToList() ?? new List<Guid>(),
                CreatedAt = step.CreatedAt,
                UpdatedAt = step.UpdatedAt,
                DeletedAt = step.DeletedAt
            };
        }

        public async Task<List<StepResponseDto>> GetStepsAsync()
        {
            var steps = await _stepRepository.GetStepsAsync();
            return steps.Select(MapToStepResponseDto).ToList();
        }

        public async Task<StepResponseDto> GetStepAsync(Guid id)
        {
            Step? step = await _stepRepository.GetStepByIdAsync(id);

            if (step == null)
            {
                throw new EntryNotFoundException($"Step with id {id} was not found.");
            }

            return MapToStepResponseDto(step);
        }

        public async Task<StepResponseDto> PostStepAsync(PostStepRequestDto payload)
        {
            Step stepToPost = new Step
            {
                Name = payload.Name
            };

            // Adaugă userii prin StepUser
            foreach (Guid userId in payload.UserIds)
            {
                User? user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    throw new EntryNotFoundException($"User with id {userId} not found.");
                }

                StepUser stepUser = new StepUser
                {
                    UserId = userId,
                    StepId = stepToPost.Id
                };
                stepToPost.Users.Add(stepUser);
            }

            // Adaugă teams prin StepTeam
            foreach (Guid teamId in payload.TeamIds)
            {
                Team? team = await _teamRepository.GetTeamByIdAsync(teamId);
                if (team == null)
                {
                    throw new EntryNotFoundException($"Team with id {teamId} not found.");
                }

                StepTeam stepTeam = new StepTeam
                {
                    TeamId = teamId,
                    StepId = stepToPost.Id
                };
                stepToPost.Teams.Add(stepTeam);
            }

            // Adaugă flows prin FlowStep
            foreach (Guid flowId in payload.FlowIds)
            {
                Flow? flow = await _flowRepository.GetFlowByIdAsync(flowId);
                if (flow == null)
                {
                    throw new EntryNotFoundException($"Flow with id {flowId} not found.");
                }

                FlowStep flowStep = new FlowStep
                {
                    FlowId = flowId,
                    StepId = stepToPost.Id
                };
                stepToPost.FlowSteps.Add(flowStep);
            }

            await _stepRepository.PostStepAsync(stepToPost);

            return MapToStepResponseDto(stepToPost);
        }

        public async Task<StepResponseDto> PatchStepAsync(Guid id, PatchStepRequestDto payload)
        {
            Step? stepToPatch = await _stepRepository.GetStepByIdAsync(id);

            if (stepToPatch == null)
            {
                throw new EntryNotFoundException($"Step with id {id} was not found.");
            }

            // Actualizează numele
            if (!string.IsNullOrEmpty(payload.Name))
            {
                stepToPatch.Name = payload.Name;
            }

            stepToPatch.UpdatedAt = DateTime.UtcNow;

            // Gestionează userii (similar cu rolurile din UserService)
            if (payload.UserIds != null)
            {
                // 1. "Șterge" toți userii existenți (setează DeletedAt)
                foreach (StepUser stepUser in stepToPatch.Users)
                {
                    stepUser.DeletedAt = DateTime.UtcNow;
                    stepUser.UpdatedAt = DateTime.UtcNow;
                }

                // 2. Adaugă userii noi
                foreach (Guid userId in payload.UserIds)
                {
                    User? user = await _userRepository.GetUserByIdAsync(userId);
                    if (user == null)
                    {
                        throw new EntryNotFoundException($"User with id {userId} not found.");
                    }

                    // Verifică dacă relația există deja și o restaurează
                    StepUser? existingStepUser = stepToPatch.Users.FirstOrDefault(su => su.UserId == userId);
                    if (existingStepUser != null)
                    {
                        existingStepUser.DeletedAt = null;
                        existingStepUser.UpdatedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        // Creează relație nouă
                        StepUser stepUserToAdd = new StepUser
                        {
                            UserId = userId,
                            StepId = stepToPatch.Id
                        };
                        stepToPatch.Users.Add(stepUserToAdd);
                    }
                }
            }

            // Gestionează teams (similar cu userii)
            if (payload.TeamIds != null)
            {
                // 1. "Șterge" toate teams existente
                foreach (StepTeam stepTeam in stepToPatch.Teams)
                {
                    stepTeam.DeletedAt = DateTime.UtcNow;
                    stepTeam.UpdatedAt = DateTime.UtcNow;
                }

                // 2. Adaugă teams noi
                foreach (Guid teamId in payload.TeamIds)
                {
                    Team? team = await _teamRepository.GetTeamByIdAsync(teamId);
                    if (team == null)
                    {
                        throw new EntryNotFoundException($"Team with id {teamId} not found.");
                    }

                    // Verifică dacă relația există deja și o restaurează
                    StepTeam? existingStepTeam = stepToPatch.Teams.FirstOrDefault(st => st.TeamId == teamId);
                    if (existingStepTeam != null)
                    {
                        existingStepTeam.DeletedAt = null;
                        existingStepTeam.UpdatedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        // Creează relație nouă
                        StepTeam stepTeamToAdd = new StepTeam
                        {
                            TeamId = teamId,
                            StepId = stepToPatch.Id
                        };
                        stepToPatch.Teams.Add(stepTeamToAdd);
                    }
                }
            }

            await _stepRepository.SaveChangesAsync();

            return MapToStepResponseDto(stepToPatch);
        }

        public async Task<StepResponseDto> DeleteStepAsync(Guid id)
        {
            Step? step = await _stepRepository.GetStepByIdAsync(id);

            if (step == null)
            {
                throw new EntryNotFoundException($"Step with id {id} was not found.");
            }

            await _stepRepository.DeleteStepAsync(step);

            return MapToStepResponseDto(step);
        }

        // ==========================================
        // DOAR FLOW MANAGEMENT (nu e redundant)
        // ==========================================

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

            if (step.FlowSteps.Any(fs => fs.FlowId == flowId && fs.DeletedAt == null))
            {
                throw new UniqueConstraintViolationException($"Step {stepId} is already part of flow {flowId}.");
            }

            // Verifică dacă există o relație ștearsă și o restaurează
            FlowStep? existingFlowStep = step.FlowSteps.FirstOrDefault(fs => fs.FlowId == flowId);
            if (existingFlowStep != null)
            {
                existingFlowStep.DeletedAt = null;
                existingFlowStep.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                step.FlowSteps.Add(new FlowStep { StepId = stepId, FlowId = flowId });
            }

            await _stepRepository.SaveChangesAsync();

            return MapToStepResponseDto(step);
        }

        public async Task<StepResponseDto> RemoveStepFromFlowAsync(Guid stepId, Guid flowId)
        {
            var step = await _stepRepository.GetStepByIdAsync(stepId);
            if (step == null)
            {
                throw new EntryNotFoundException($"Step {stepId} not found.");
            }

            var flowStep = step.FlowSteps.FirstOrDefault(fs => fs.FlowId == flowId && fs.DeletedAt == null);
            if (flowStep == null)
            {
                throw new InvalidOperationException($"Step {stepId} is not part of flow {flowId}.");
            }

            flowStep.DeletedAt = DateTime.UtcNow;
            flowStep.UpdatedAt = DateTime.UtcNow;

            await _stepRepository.SaveChangesAsync();

            return MapToStepResponseDto(step);
        }

        public async Task<StepResponseDto> RestoreStepToFlowAsync(Guid stepId, Guid flowId)
        {
            var step = await _stepRepository.GetStepByIdAsync(stepId);
            if (step == null)
            {
                throw new EntryNotFoundException($"Step {stepId} not found.");
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
            deletedFlowStep.UpdatedAt = DateTime.UtcNow;

            await _stepRepository.SaveChangesAsync();

            return MapToStepResponseDto(step);
        }

        public async Task<PagedResponseDto<StepResponseDto>> GetAllStepsQueriedAsync(QueriedStepRequestDto payload)
        {
            QueryParams? parameters = payload.QueryParams?.ToQueryParams();

            (List<Step> data, int totalCount) = await _stepRepository.GetAllStepsQueriedAsync(payload.Name, parameters);

            return new PagedResponseDto<StepResponseDto>
            {
                Data = data.Select(MapToStepResponseDto).ToList(),
                TotalCount = totalCount,
                Page = parameters?.Page ?? 1,
                PageSize = parameters?.PageSize ?? totalCount,
            };
        }
    }
}