using FlowManager.Application.Interfaces;
using FlowManager.Domain.Entities;
using FlowManager.Domain.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace FlowManager.Infrastructure.Services
{
    public class FlowService : IFlowService
    {
        private readonly IFlowRepository _flowRepository;

        public FlowService(IFlowRepository flowRepository)
        {
            _flowRepository = flowRepository;   
        }

        public async Task<IEnumerable<Flow>> GetAllFlowsAsync()
        {
            return await _flowRepository.GetAllFlowsAsync();
        }

        public async Task<Flow?> GetFlowByIdAsync(Guid id)
        {
            return await _flowRepository.GetFlowByIdAsync(id);
        }

        public async Task<Flow> CreateFlowAsync(Flow flow)
        {
            return await _flowRepository.CreateFlowAsync(flow);
        }

        public async Task<bool> UpdateFlowAsync(Guid id, Flow flow)
        {
            return await _flowRepository.UpdateFlowAsync(id, flow);
        }

        public async Task<bool> DeleteFlowAsync(Guid id)
        {
            return await _flowRepository.DeleteFlowAsync(id);
        }
    }
}
