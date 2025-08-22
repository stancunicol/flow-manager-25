using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowManager.Domain.Dtos;
using FlowManager.Domain.Entities;

namespace FlowManager.Domain.IRepositories
{
    public interface IStepRepository
    {
        Task<IEnumerable<Step>> GetStepsAsync();
        Task<Step?> GetStepByIdAsync(Guid id, bool includeDeleted = false, bool includeDeletedStepUser = false, bool includeDeletedStepTeams = false);
        Task<IEnumerable<Step>> GetStepsByFlowAsync(Guid flowId);
        Task<Step> PostStepAsync(Step step);
        Task<Step> DeleteStepAsync(Step step);
        Task SaveChangesAsync();
        Task<(List<Step> Steps, int TotalCount)> GetAllStepsIncludeUsersAndTeamsQueriedAsync(string? name, QueryParams? parameters);
    }
}
