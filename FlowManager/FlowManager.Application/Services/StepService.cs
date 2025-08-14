using FlowManager.Application.Interfaces;
using FlowManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using FlowManager.Domain.IRepositories;

namespace FlowManager.Application.Services
{
    public class StepService : IStepService
    {
        private readonly IStepRepository _stepRepository;
        public StepService(IStepRepository stepRepository) 
        {
            _stepRepository = stepRepository;
        }
        public async Task<IEnumerable<Step>> GetSteps()
        {
            return await _stepRepository.GetSteps();
        }
        public async Task<Step?> GetStep(Guid id)
        {
            return await _stepRepository.GetStep(id);
        }
        public async Task<IEnumerable<Step>> GetStepsByFlow(Guid flowId)
        {
            return await _stepRepository.GetStepsByFlow(flowId);
        }
        public async Task<Step> PostStep(Step step)
        {
            return await _stepRepository.PostStep(step);
        }
        public async Task<bool> PutStep(Guid id, Step step)
        {
            return await _stepRepository.PutStep(id, step);
        }
        public async Task<bool> DeleteStep(Guid id)
        {
            return await _stepRepository.DeleteStep(id);
        }
        public async Task<bool> AssignUserToStep(Guid id, Guid userId)
        {
            return await _stepRepository.AssignUserToStep(id, userId);
        }
        public async Task<bool> UnassignUserFromStep(Guid id, Guid userId)
        {
            return await _stepRepository.UnassignUserFromStep(id, userId);
        }
        public async Task<bool> AddStepToFlow(Guid stepId, Guid flowId)
        {
            return await _stepRepository.AddStepToFlow(stepId, flowId);
        }
        public async Task<bool> RemoveStepFromFlow(Guid stepId, Guid flowId)
        {
            return await _stepRepository.RemoveStepFromFlow(stepId, flowId);
        }
    }
}
