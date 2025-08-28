using System;
using System.Collections.Generic;
using System.Linq;
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
    public class StepRepository : IStepRepository
    {
        private readonly AppDbContext _context;

        public StepRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Step>> GetStepsAsync()
        {
            return await _context.Steps.Where(s => s.DeletedAt == null)
                .Include(s => s.Users)
                    .ThenInclude(su => su.User)
                .Include(s => s.Teams)
                    .ThenInclude(st => st.Team)
                        .ThenInclude(t => t.Users)
                .ToListAsync();
        }

        public async Task<Step?> GetStepByIdAsync(Guid stepId, bool includeDeletedStepUser = false, bool includeDeletedStepTeams = false, bool includeUsers = false, bool includeTeams = false)
        {
            var query = _context.Steps.AsQueryable();

            if (includeUsers)
            {
                if (includeDeletedStepUser)
                {
                    query = query.Include(s => s.Users)
                                 .ThenInclude(su => su.User);
                }
                else
                {
                    query = query.Include(s => s.Users.Where(u => u.DeletedAt == null))
                                 .ThenInclude(su => su.User);
                }
            }

            if (includeTeams)
            {
                if (includeDeletedStepTeams)
                {
                    query = query.Include(s => s.Teams)
                                 .ThenInclude(st => st.Team)
                                 .ThenInclude(t => t.Users)
                                 .ThenInclude(ut => ut.User);
                }
                else
                {
                    query = query.Include(s => s.Teams.Where(t => t.DeletedAt == null))
                                 .ThenInclude(st => st.Team)
                                 .ThenInclude(t => t.Users)
                                 .ThenInclude(ut => ut.User);
                }
            }

            return await query.FirstOrDefaultAsync(s => s.Id == stepId);
        }

        public async Task<IEnumerable<Step>> GetStepsByFlowAsync(Guid flowId)
        {
            return await _context.FlowSteps
                .Where(fs => fs.FlowId == flowId)
                .Include(fs => fs.AssignedUsers)
                    .ThenInclude(fsu => fsu.User)
                .Include(fs => fs.AssignedTeams)
                    .ThenInclude(fst => fst.Team)
                        .ThenInclude(t => t.Users)
                .Select(fs => fs.Step)
                .ToListAsync();
        }

        public async Task<Step> PostStepAsync(Step step)
        {
            _context.Steps.Add(step);
            await _context.SaveChangesAsync();

            return step;
        }

        public async Task<Step> DeleteStepAsync(Step step)
        {
            step.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return step;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<(List<Step> Steps, int TotalCount)> GetAllStepsIncludeUsersAndTeamsQueriedAsync(string? name, QueryParams? parameters)
        {
            IQueryable<Step> query = _context.Steps
    .Include(s => s.Users
        .Where(su => su.DeletedAt == null))
        .ThenInclude(su => su.User)
    .Include(s => s.Teams
        .Where(st => st.DeletedAt == null))
        .ThenInclude(st => st.Team)
            .ThenInclude(t => t.Users)
                .ThenInclude(tu => tu.User);


            query = query.Where(s => s.DeletedAt == null);

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(s => s.Name.Contains(name));
            }

            int totalCount = query.Count();

            if (parameters == null)
            {
                return (await query.ToListAsync(), totalCount);
            }

            if (parameters.SortBy != null)
            {
                if (parameters.SortDescending is bool sortDesc)
                    query = query.ApplySorting<Step>(parameters.SortBy, sortDesc);
                else
                    query = query.ApplySorting<Step>(parameters.SortBy, false);
            }

            if (parameters.Page == null || parameters.Page < 0 ||
               parameters.PageSize == null || parameters.PageSize < 0)
            {
                return (await query.ToListAsync(), totalCount);
            }
            else
            {
                List<Step> steps = await query.Skip((int)parameters.PageSize * ((int)parameters.Page - 1))
                                               .Take((int)parameters.PageSize)
                                               .ToListAsync();
                return (steps, totalCount);
            }
        }

        public async Task AddStepUserAsync(StepUser stepUser)
        {
            _context.StepUsers.Add(stepUser);
            await _context.SaveChangesAsync();
        }

        public void AttachStepUser(StepUser stepUser)
        {
            var entry = _context.Entry(stepUser);
            if (entry.State == EntityState.Detached)
            {
                _context.StepUsers.Attach(stepUser);
            }
        }

        public void DettachStepUser(StepUser stepUser)
        {
            _context.Update(stepUser);
        }
    }
}