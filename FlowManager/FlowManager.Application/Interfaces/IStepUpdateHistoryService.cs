using FlowManager.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Application.Interfaces
{
    public interface IStepUpdateHistoryService
    {
        Task<IEnumerable<StepUpdateHistory>> GetStepUpdateHistories();
        Task<StepUpdateHistory?> GetStepUpdateHistory(Guid id);
        Task<IEnumerable<StepUpdateHistory>> GetHistoriesByStep(Guid stepId);
        Task<IEnumerable<StepUpdateHistory>> GetHistoriesByUser(Guid userId);
        Task<StepUpdateHistory> PostStepUpdateHistory(StepUpdateHistory stepUpdateHistory);
        Task<bool> PutStepUpdateHistory(Guid id, StepUpdateHistory stepUpdateHistory);
        Task<bool> DeleteStepUpdateHistory(Guid id);
    }
}
