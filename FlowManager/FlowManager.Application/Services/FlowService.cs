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

        public Task<Flow> CreateFlowAsync(Flow flow)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteFlowAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Flow>> GetAllFlowsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Flow?> GetFlowByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateFlowAsync(Guid id, Flow flow)
        {
            throw new NotImplementedException();
        }
    }
}
