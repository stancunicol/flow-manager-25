using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowManager.Domain.Entities;

namespace FlowManager.Domain.IRepositories
{
    public interface IFlowRepository
    {
        Task<List<Flow>> GetAllFlowsAsync();
        Task<Flow?> GetFlowByIdAsync(Guid id);
        Task<Flow> CreateFlowAsync(Flow flow);
        Task<bool> UpdateFlowAsync(Guid id, Flow flow);
        Task<bool> DeleteFlowAsync(Guid id);
    }
}
