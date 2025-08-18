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
            return await _context.Steps
                .Where(s => s.DeletedAt == null)
                .Include(s => s.Users.Where(su => su.DeletedAt == null)) // StepUser collection
                    .ThenInclude(su => su.User)
                .Include(s => s.Teams.Where(st => st.DeletedAt == null)) // StepTeam collection
                    .ThenInclude(st => st.Team)
                .Include(s => s.FlowSteps)
                    .ThenInclude(fs => fs.Flow)
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<Step?> GetStepByIdAsync(Guid id)
        {
            return await _context.Steps
                .Where(s => s.DeletedAt == null)
                .Include(s => s.Users.Where(su => su.DeletedAt == null)) // StepUser collection
                    .ThenInclude(su => su.User)
                .Include(s => s.Teams.Where(st => st.DeletedAt == null)) // StepTeam collection
                    .ThenInclude(st => st.Team)
                        .ThenInclude(t => t.Users.Where(u => u.DeletedAt == null))
                .Include(s => s.FlowSteps)
                    .ThenInclude(fs => fs.Flow)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Step?> GetStepByIdSimpleAsync(Guid id)
        {
            // Versiune simplă fără includes pentru operațiuni rapide
            return await _context.Steps
                .Where(s => s.DeletedAt == null)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IEnumerable<Step>> GetStepsByFlowAsync(Guid flowId)
        {
            return await _context.FlowSteps
                .Where(fs => fs.FlowId == flowId)
                .Include(fs => fs.Step)
                    .ThenInclude(s => s.Users.Where(su => su.DeletedAt == null))
                        .ThenInclude(su => su.User)
                .Include(fs => fs.Step)
                    .ThenInclude(s => s.Teams.Where(st => st.DeletedAt == null))
                        .ThenInclude(st => st.Team)
                .Select(fs => fs.Step)
                .Where(s => s.DeletedAt == null)
                .OrderBy(s => s.Name)
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
            step.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return step;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<(List<Step> Steps, int TotalCount)> GetAllStepsQueriedAsync(string? name, QueryParams? parameters)
        {
            IQueryable<Step> query = _context.Steps
                .Where(s => s.DeletedAt == null)
                .Include(s => s.Users.Where(su => su.DeletedAt == null))
                    .ThenInclude(su => su.User)
                .Include(s => s.Teams.Where(st => st.DeletedAt == null))
                    .ThenInclude(st => st.Team)
                .Include(s => s.FlowSteps)
                    .ThenInclude(fs => fs.Flow);

            // Filtering
            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(s => s.Name.Contains(name));
            }

            int totalCount = await query.CountAsync();

            if (parameters == null)
            {
                return (await query.OrderBy(s => s.Name).ToListAsync(), totalCount);
            }

            // Sorting - urmând stilul tău original
            if (parameters.SortBy != null)
            {
                if (parameters.SortDescending is bool sortDesc)
                    query = query.ApplySorting<Step>(parameters.SortBy, sortDesc);
                else
                    query = query.ApplySorting<Step>(parameters.SortBy, false);
            }

            // Pagination - urmând stilul tău original
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

        // Metode noi pentru managementul relațiilor

        public async Task<List<Step>> GetStepsByUserAsync(Guid userId)
        {
            return await _context.StepUsers
                .Where(su => su.DeletedAt == null && su.UserId == userId)
                .Include(su => su.Step)
                    .ThenInclude(s => s.Teams.Where(st => st.DeletedAt == null))
                        .ThenInclude(st => st.Team)
                .Select(su => su.Step)
                .Where(s => s.DeletedAt == null)
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<List<Step>> GetStepsByTeamAsync(Guid teamId)
        {
            return await _context.StepTeams
                .Where(st => st.DeletedAt == null && st.TeamId == teamId)
                .Include(st => st.Step)
                    .ThenInclude(s => s.Users.Where(su => su.DeletedAt == null))
                        .ThenInclude(su => su.User)
                .Select(st => st.Step)
                .Where(s => s.DeletedAt == null)
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<bool> StepExistsByNameAsync(string name, Guid? excludeId = null)
        {
            var query = _context.Steps
                .Where(s => s.DeletedAt == null && s.Name.ToLower() == name.ToLower());

            if (excludeId.HasValue)
            {
                query = query.Where(s => s.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<int> GetUsersCountInStepAsync(Guid stepId)
        {
            return await _context.StepUsers
                .Where(su => su.DeletedAt == null && su.StepId == stepId)
                .CountAsync();
        }

        public async Task<int> GetTeamsCountInStepAsync(Guid stepId)
        {
            return await _context.StepTeams
                .Where(st => st.DeletedAt == null && st.StepId == stepId)
                .CountAsync();
        }

        public async Task<int> GetFlowsCountInStepAsync(Guid stepId)
        {
            return await _context.FlowSteps
                .Where(fs => fs.StepId == stepId)
                .CountAsync();
        }
    }
}