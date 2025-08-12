using FlowManager.Domain.Entities;

namespace FlowManager.Application.Interfaces
{
    public interface IFlowService
    {
        Task<IEnumerable<Flow>> GetAllFlowsAsync();
        Task<Flow?> GetFlowByIdAsync(Guid id);
        Task<Flow> CreateFlowAsync(Flow flow);
        Task<bool> UpdateFlowAsync(Guid id, Flow flow);
        Task<bool> DeleteFlowAsync(Guid id);
    }
}