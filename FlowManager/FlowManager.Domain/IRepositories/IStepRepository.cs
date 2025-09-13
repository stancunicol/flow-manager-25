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
        Task<Step?> GetStepByIdAsync(Guid stepId, bool includeDeletedStepUser = false, bool includeDeletedStepTeams = false, bool includeUsers = false, bool includeTeams = false);
        Task<IEnumerable<Step>> GetStepsByFlowAsync(Guid flowId);
        Task<Step> PostStepAsync(Step step);
        Task<Step> DeleteStepAsync(Step step);
        Task SaveChangesAsync();
        Task<(List<Step> Steps, int TotalCount)> GetAllStepsQueriedAsync(Guid moderatorId, string? name, QueryParams? parameters);
        Task<(List<Step> Steps, int TotalCount)> GetAllStepsIncludeUsersAndTeamsQueriedAsync(Guid moderatorId, string? name, QueryParams? parameters);
    }
}
