using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowManager.Domain.Dtos;
using FlowManager.Domain.Entities;

namespace FlowManager.Domain.IRepositories
{
    public interface IFlowRepository
    {
        Task<List<Flow>> GetAllFlowsAsync();
        Task<(List<Flow> Data,int TotalCount)> GetAllFlowsQueriedAsync(string? name, QueryParams? parameters);
        Task<Flow?> GetFlowByIdAsync(Guid id);
        Task<Flow?> GetFlowIncludeDeletedStepsByIdAsync(Guid id);
        Task<Flow> CreateFlowAsync(Flow flow);
        Task<Flow?> GetFlowByIdIncludeStepsAsync(Guid flowId,Guid moderatorId);
        Task<Flow?> GetFlowByNameAsync(string flowName);
        Task<Flow?> GetFlowByFormTemplateIdAsync(Guid formTemplateId);
        Task SaveChangesAsync();
    }
}
