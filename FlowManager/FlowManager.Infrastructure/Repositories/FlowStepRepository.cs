using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using FlowManager.Domain.Dtos;
using FlowManager.Domain.Entities;
using FlowManager.Domain.IRepositories;
using FlowManager.Infrastructure.Context;
using FlowManager.Infrastructure.Utils;
using Microsoft.EntityFrameworkCore;

namespace FlowManager.Infrastructure.Repositories
{
    public class FlowStepRepository: IFlowStepRepository
    {
        private readonly AppDbContext _context;

        public FlowStepRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(List<FlowStep> Data, int TotalCount)> GetAllFlowStepsQueriedAsync(string? name, QueryParams? parameters)
        {
            IQueryable<FlowStep> query = _context.FlowSteps
                .Include(fs => fs.Flow)
                .Include(fs => fs.Step)
                .Where(fs => fs.DeletedAt == null);

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(fs => fs.Flow.Name.ToUpper().Contains(name.ToUpper()) || fs.Step.Name.ToUpper().Contains(name.ToUpper()));
            }
            int totalCount = await query.CountAsync();

            if (parameters != null)
            {
                query = query
                    .Skip((int)((parameters.PageNumber - 1) * parameters.PageSize))
                    .Take((int)parameters.PageSize);
            }
            List<FlowStep> data = await query.ToListAsync();
            return (data, totalCount);
        }

        public async Task<List<FlowStep>> GetAllFlowStepsAsync()
        {
            return await _context.FlowSteps
                .Include(fs => fs.Flow)
                .Include(fs => fs.Step)
                .ToListAsync();
        }

        public async Task<FlowStep?> GetFlowStepByIdAsync(Guid id, bool includeDeletedFlowStepUsers = false, bool includeDeletedFlowStepTeams = false)
        {
            var query = _context.FlowSteps.AsQueryable();

            if (includeDeletedFlowStepUsers)
            {
                query = query.Include(fs => fs.AssignedUsers)
                            .ThenInclude(fsu => fsu.User);
            }
            else
            {
                query = query.Include(fs => fs.AssignedUsers.Where(fsu => fsu.DeletedAt == null))
                            .ThenInclude(fsu => fsu.User);
            }

            if (includeDeletedFlowStepTeams)
            {
                query = query.Include(fs => fs.AssignedTeams)
                            .ThenInclude(fst => fst.Team)
                                .ThenInclude(t => t.Users.Where(tu => tu.DeletedAt == null))
                                    .ThenInclude(tu => tu.User);
            }
            else
            {
                query = query.Include(fs => fs.AssignedTeams.Where(fst => fst.DeletedAt == null))
                            .ThenInclude(fst => fst.Team)
                                .ThenInclude(t => t.Users.Where(tu => tu.DeletedAt == null))
                                    .ThenInclude(tu => tu.User);
            }

            return await query
                .Where(fs => fs.DeletedAt == null && fs.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<FlowStep> AddFlowStepAsync(FlowStep flowStep)
        {
            _context.FlowSteps.Add(flowStep);
            await _context.SaveChangesAsync();
            return flowStep;
        }

        public async Task<FlowStep> UpdateFlowStepAsync(FlowStep flowStep)
        {
            await _context.SaveChangesAsync();
            return flowStep;
        }

        public async Task DeleteFlowStepAsync(FlowStep flowStep)
        {
            flowStep.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<FlowStep?> GetFlowStepByFlowIdAndStepIdAsync(Guid flowId, Guid stepId)
        {
            return await _context.FlowSteps
                .Include(fs => fs.Flow)
                .Include(fs => fs.Step)
                .FirstOrDefaultAsync(fs => fs.FlowId == flowId && fs.StepId == stepId);
        }

        public async Task UpdateFlowStepUsersAsync(Guid flowStepId, List<Guid> userIds)
        {
            var existingUsers = await _context.FlowStepUsers
                .Where(fsu => fsu.FlowStepId == flowStepId && fsu.DeletedAt == null)
                .ToListAsync();

            foreach (var flowStepUser in existingUsers)
            {
                flowStepUser.DeletedAt = DateTime.UtcNow;
                flowStepUser.UpdatedAt = DateTime.UtcNow;
                _context.Entry(flowStepUser).State = EntityState.Modified;
            }

            foreach (var userId in userIds)
            {
                var existing = await _context.FlowStepUsers
                    .FirstOrDefaultAsync(fsu => fsu.FlowStepId == flowStepId && fsu.UserId == userId);

                if (existing != null)
                {
                    existing.DeletedAt = null;
                    existing.UpdatedAt = DateTime.UtcNow;
                    _context.Entry(existing).State = EntityState.Modified;
                }
                else
                {
                    var newFlowStepUser = new FlowStepUser
                    {
                        UserId = userId,
                        FlowStepId = flowStepId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    await _context.FlowStepUsers.AddAsync(newFlowStepUser);
                }
            }
        }

        public async Task UpdateFlowStepTeamsAsync(Guid flowStepId, List<Guid> teamIds)
        {
            var existingTeams = await _context.FlowStepTeams
                .Where(fst => fst.FlowStepId == flowStepId && fst.DeletedAt == null)
                .ToListAsync();

            foreach (var flowStepTeam in existingTeams)
            {
                flowStepTeam.DeletedAt = DateTime.UtcNow;
                flowStepTeam.UpdatedAt = DateTime.UtcNow;
                _context.Entry(flowStepTeam).State = EntityState.Modified;
            }

            foreach (var teamId in teamIds)
            {
                var existing = await _context.FlowStepTeams
                    .FirstOrDefaultAsync(fst => fst.FlowStepId == flowStepId && fst.TeamId == teamId);

                if (existing != null)
                {
                    existing.DeletedAt = null;
                    existing.UpdatedAt = DateTime.UtcNow;
                    _context.Entry(existing).State = EntityState.Modified;
                }
                else
                {
                    var newFlowStepTeam = new FlowStepTeam
                    {
                        TeamId = teamId,
                        FlowStepId = flowStepId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    await _context.FlowStepTeams.AddAsync(newFlowStepTeam);
                }
            }
        }
    }
}
