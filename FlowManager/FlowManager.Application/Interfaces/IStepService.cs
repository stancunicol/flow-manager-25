using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowManager.Domain.Entities;

namespace FlowManager.Application.Interfaces
{
    public interface IStepService
    {
        Task<IEnumerable<Step>> GetSteps();
        Task<Step?> GetStep(Guid id);
        Task<IEnumerable<Step>> GetStepsByFlow(Guid flowId);
        Task<Step> PostStep(Step step);
        Task<bool> PutStep(Guid id, Step step);
        Task<bool> DeleteStep(Guid id);
        Task<bool> AssignUserToStep(Guid id, Guid userId);
        Task<bool> UnassignUserFromStep(Guid id, Guid userId);
        Task<bool> AddStepToFlow(Guid stepId, Guid flowId, int order = 0);
        Task<bool> RemoveStepFromFlow(Guid stepId, Guid flowId);



    }
}
