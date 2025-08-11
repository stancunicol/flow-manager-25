using FlowManager.Application.Interfaces;
using FlowManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlowManager.Infrastructure.Services
{
    //public class StepService : IStepService
    //{
    //    private readonly AppDbContext _context;

    //    public StepService(AppDbContext context)
    //    {
    //        _context = context;
    //    }

    //    public async Task<IEnumerable<Step>> GetSteps()
    //    {
    //        return await _context.Steps
    //            .Include(s => s.FlowSteps).ThenInclude(fs => fs.Flow)
    //            .Include(s => s.StepUsers).ThenInclude(su => su.User)
    //            .Include(s => s.UpdateHistories)
    //            .ToListAsync();
    //    }

    //    public async Task<Step?> GetStep(Guid id)
    //    {
    //        return await _context.Steps
    //            .Include(s => s.FlowSteps).ThenInclude(fs => fs.Flow)
    //            .Include(s => s.StepUsers).ThenInclude(su => su.User)
    //            .Include(s => s.UpdateHistories)
    //            .FirstOrDefaultAsync(s => s.Id == id);
    //    }

    //    public async Task<IEnumerable<Step>> GetStepsByFlow(Guid flowId)
    //    {
    //        return await _context.Steps
    //            .Include(s => s.FlowSteps).ThenInclude(fs => fs.Flow)
    //            .Include(s => s.StepUsers).ThenInclude(su => su.User)
    //            .Include(s => s.UpdateHistories)
    //            .Where(s => s.FlowSteps.Any(fs => fs.FlowId == flowId))
    //            .ToListAsync();
    //    }

    //    public async Task<Step> PostStep(Step step)
    //    {
    //        step.Id = Guid.NewGuid();
    //        step.CreatedAt = DateTime.UtcNow;
    //        step.UpdatedAt = DateTime.UtcNow;

    //        _context.Steps.Add(step);
    //        await _context.SaveChangesAsync();

    //        return step;
    //    }

    //    public async Task<bool> PutStep(Guid id, Step step)
    //    {
    //        if (id != step.Id)
    //            return false;

    //        step.UpdatedAt = DateTime.UtcNow;
    //        _context.Entry(step).State = EntityState.Modified;

    //        try
    //        {
    //            await _context.SaveChangesAsync();
    //            return true;
    //        }
    //        catch (DbUpdateConcurrencyException)
    //        {
    //            return _context.Steps.Any(e => e.Id == id);
    //        }
    //    }

    //    public async Task<bool> DeleteStep(Guid id)
    //    {
    //        var step = await _context.Steps.FindAsync(id);
    //        if (step == null)
    //            return false;

    //        _context.Steps.Remove(step);
    //        await _context.SaveChangesAsync();
    //        return true;
    //    }

    //    public async Task<bool> AssignUserToStep(Guid id, Guid userId)
    //    {
    //        var step = await _context.Steps
    //            .Include(s => s.StepUsers)
    //            .FirstOrDefaultAsync(s => s.Id == id);

    //        var user = await _context.Users.FindAsync(userId);

    //        if (step == null || user == null)
    //            return false;

    //        if (!step.StepUsers.Any(su => su.UserId == userId))
    //        {
    //            step.StepUsers.Add(new StepUser
    //            {
    //                StepId = id,
    //                UserId = userId
    //            });

    //            await _context.SaveChangesAsync();
    //        }

    //        return true;
    //    }

    //    public async Task<bool> UnassignUserFromStep(Guid id, Guid userId)
    //    {
    //        var step = await _context.Steps
    //            .Include(s => s.StepUsers)
    //            .FirstOrDefaultAsync(s => s.Id == id);

    //        if (step == null)
    //            return false;

    //        var link = step.StepUsers.FirstOrDefault(su => su.UserId == userId);
    //        if (link != null)
    //        {
    //            step.StepUsers.Remove(link);
    //            await _context.SaveChangesAsync();
    //        }

    //        return true;
    //    }

    //    public async Task<bool> AddStepToFlow(Guid stepId, Guid flowId, int order = 0)
    //    {
    //        var step = await _context.Steps.FindAsync(stepId);
    //        var flow = await _context.Flows.FindAsync(flowId);

    //        if (step == null || flow == null)
    //            return false;

    //        var existingFlowStep = await _context.FlowSteps
    //            .FirstOrDefaultAsync(fs => fs.StepId == stepId && fs.FlowId == flowId);

    //        if (existingFlowStep != null)
    //            return false; // Already exists

    //        var flowStep = new FlowStep
    //        {
    //            StepId = stepId,
    //            FlowId = flowId,
    //            Order = order,
    //            CreatedAt = DateTime.UtcNow
    //        };

    //        _context.FlowSteps.Add(flowStep);
    //        await _context.SaveChangesAsync();
    //        return true;
    //    }

    //    public async Task<bool> RemoveStepFromFlow(Guid stepId, Guid flowId)
    //    {
    //        var flowStep = await _context.FlowSteps
    //            .FirstOrDefaultAsync(fs => fs.StepId == stepId && fs.FlowId == flowId);

    //        if (flowStep == null)
    //            return false;

    //        _context.FlowSteps.Remove(flowStep);
    //        await _context.SaveChangesAsync();
    //        return true;
    //    }
    //}
}
