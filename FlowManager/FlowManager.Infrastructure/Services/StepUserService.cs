using FlowManager.Application.Interfaces;
using FlowManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlowManager.Infrastructure.Services
{
    //public class StepUserService : IStepUserService
    //{
    //    private readonly AppDbContext _context;

    //    public StepUserService(AppDbContext context)
    //    {
    //        _context = context;
    //    }

    //    public async Task<IEnumerable<StepUser>> GetAll()
    //    {
    //        return await _context.StepUsers
    //            .Include(su => su.Step)
    //            .Include(su => su.User)
    //            .ToListAsync();
    //    }

    //    public async Task<IEnumerable<User>> GetUsersByStep(Guid stepId)
    //    {
    //        return await _context.StepUsers
    //            .Where(su => su.StepId == stepId)
    //            .Include(su => su.User)
    //            .Select(su => su.User)
    //            .ToListAsync();
    //    }

    //    public async Task<IEnumerable<Step>> GetStepsByUser(Guid userId)
    //    {
    //        return await _context.StepUsers
    //            .Where(su => su.UserId == userId)
    //            .Include(su => su.Step)
    //            .Select(su => su.Step)
    //            .ToListAsync();
    //    }

    //    public async Task<bool> AssignUser(Guid stepId, Guid userId)
    //    {
    //        var exists = await _context.StepUsers.AnyAsync(su => su.StepId == stepId && su.UserId == userId);
    //        if (exists) return true;

    //        _context.StepUsers.Add(new StepUser
    //        {
    //            StepId = stepId,
    //            UserId = userId,
    //            AssignedAt = DateTime.UtcNow
    //        });

    //        await _context.SaveChangesAsync();
    //        return true;
    //    }

    //    public async Task<bool> UnassignUser(Guid stepId, Guid userId)
    //    {
    //        var link = await _context.StepUsers
    //            .FirstOrDefaultAsync(su => su.StepId == stepId && su.UserId == userId);

    //        if (link == null) return false;

    //        _context.StepUsers.Remove(link);
    //        await _context.SaveChangesAsync();
    //        return true;
    //    }
    //}
}
