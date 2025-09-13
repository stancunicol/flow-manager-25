using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowManager.Domain.Dtos;
using FlowManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlowManager.Domain.IRepositories
{
    public interface IFlowStepRepository
    {
        Task<(List<FlowStep> Data, int TotalCount)> GetAllFlowStepsQueriedAsync(string? name, QueryParams? parameters);
        Task<List<FlowStep>> GetAllFlowStepsAsync();
        Task<FlowStep> AddFlowStepAsync(FlowStep flowStep);
        Task<FlowStep> UpdateFlowStepAsync(FlowStep flowStep);
        Task DeleteFlowStepAsync(FlowStep flowStep);
        Task SaveChangesAsync();
        Task<FlowStep?> GetFlowStepByIdAsync(Guid id, bool includeDeletedFlowStepUsers = false, bool includeDeletedFlowStepTeams = false);
        Task<FlowStep?> GetFlowStepByFlowIdAndStepIdAsync(Guid flowId, Guid stepId);
        Task UpdateFlowStepUsersAsync(Guid flowStepId, List<Guid> userIds);
        Task UpdateFlowStepTeamsAsync(Guid flowStepId, List<Guid> teamIds);
    }
}
