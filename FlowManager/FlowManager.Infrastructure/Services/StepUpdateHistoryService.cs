using FlowManager.Application.Interfaces;
using FlowManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Infrastructure.Services
{
    //public class StepUpdateHistoryService : IStepUpdateHistoryService
    //{
    //    private readonly AppDbContext _context;

    //    public StepUpdateHistoryService(AppDbContext context)
    //    {
    //        _context = context;
    //    }

    //    public async Task<IEnumerable<StepUpdateHistory>> GetStepUpdateHistories()
    //    {
    //        return await _context.StepUpdateHistories
    //            .Include(suh => suh.Step)
    //            .Include(suh => suh.User)
    //            .OrderByDescending(suh => suh.UpdatedAt)
    //            .ToListAsync();
    //    }

    //    public async Task<StepUpdateHistory?> GetStepUpdateHistory(Guid id)
    //    {
    //        return await _context.StepUpdateHistories
    //            .Include(suh => suh.Step)
    //            .Include(suh => suh.User)
    //            .FirstOrDefaultAsync(suh => suh.Id == id);
    //    }

    //    public async Task<IEnumerable<StepUpdateHistory>> GetHistoriesByStep(Guid stepId)
    //    {
    //        return await _context.StepUpdateHistories
    //            .Include(suh => suh.Step)
    //            .Include(suh => suh.User)
    //            .Where(suh => suh.StepId == stepId)
    //            .OrderByDescending(suh => suh.UpdatedAt)
    //            .ToListAsync();
    //    }

    //    public async Task<IEnumerable<StepUpdateHistory>> GetHistoriesByUser(Guid userId)
    //    {
    //        return await _context.StepUpdateHistories
    //            .Include(suh => suh.Step)
    //            .Include(suh => suh.User)
    //            .Where(suh => suh.UserId == userId)
    //            .OrderByDescending(suh => suh.UpdatedAt)
    //            .ToListAsync();
    //    }

    //    public async Task<StepUpdateHistory> PostStepUpdateHistory(StepUpdateHistory stepUpdateHistory)
    //    {
    //        stepUpdateHistory.Id = Guid.NewGuid();
    //        stepUpdateHistory.UpdatedAt = DateTime.UtcNow;

    //        _context.StepUpdateHistories.Add(stepUpdateHistory);
    //        await _context.SaveChangesAsync();
    //        return stepUpdateHistory;
    //    }

    //    public async Task<bool> PutStepUpdateHistory(Guid id, StepUpdateHistory stepUpdateHistory)
    //    {
    //        if (id != stepUpdateHistory.Id)
    //            return false;

    //        _context.Entry(stepUpdateHistory).State = EntityState.Modified;

    //        try
    //        {
    //            await _context.SaveChangesAsync();
    //            return true;
    //        }
    //        catch (DbUpdateConcurrencyException)
    //        {
    //            return _context.StepUpdateHistories.Any(e => e.Id == id);
    //        }
    //    }

    //    public async Task<bool> DeleteStepUpdateHistory(Guid id)
    //    {
    //        var history = await _context.StepUpdateHistories.FindAsync(id);
    //        if (history == null) return false;

    //        _context.StepUpdateHistories.Remove(history);
    //        await _context.SaveChangesAsync();
    //        return true;
    //    }
    //}
}
