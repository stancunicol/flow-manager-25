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
        Task<Step?> GetStepByIdAsync(Guid id);
        Task<IEnumerable<Step>> GetStepsByFlowAsync(Guid flowId);
        Task<Step> PostStepAsync(Step step);
        Task<Step> DeleteStepAsync(Step step);
        Task SaveChangesAsync();
        Task<(List<Step> Steps, int TotalCount)> GetAllStepsQueriedAsync(string? name, QueryParams? parameters);

        // Metode pentru managementul optimizat al relațiilor
        Task<Step> ReplaceStepUsersAsync(Guid stepId, List<Guid> newUserIds);
        Task<Step> ReplaceStepTeamsAsync(Guid stepId, List<Guid> newTeamIds);
        Task<Step> UpdateStepNameAsync(Guid stepId, string newName);

        // Metode pentru validare
        Task<List<Guid>> ValidateUsersExistAsync(List<Guid> userIds);
        Task<List<Guid>> ValidateTeamsExistAsync(List<Guid> teamIds);

        // Metode pentru loading optimizat
        Task<Step?> GetStepByIdForPatchAsync(Guid id);
        Task<Step?> GetStepByIdForDisplayAsync(Guid id);
    }
}
