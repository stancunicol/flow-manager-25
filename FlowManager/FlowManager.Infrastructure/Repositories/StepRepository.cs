using FlowManager.Domain.Dtos;
using FlowManager.Domain.Entities;
using FlowManager.Domain.Exceptions;
using FlowManager.Domain.IRepositories;
using FlowManager.Infrastructure.Context;
using FlowManager.Infrastructure.Utils;
using FlowManager.Shared.DTOs.Responses;
using FlowManager.Shared.DTOs.Responses.Step;
using FlowManager.Shared.DTOs.Responses.Team;
using FlowManager.Shared.DTOs.Responses.User;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    .ThenInclude(u => u.Teams)
                        .ThenInclude(ut => ut.Team)
                .ToListAsync();
        }

        public async Task<Step?> GetStepByIdAsync(Guid stepId, bool includeDeletedStepUser = false, bool includeDeletedStepTeams = false, bool includeUsers = false, bool includeTeams = false)
        {
            var query = _context.Steps.AsQueryable();

            if (includeUsers)
            {
                if (includeDeletedStepUser)
                {
                    query = query.Include(s => s.Users);
                }
                else
                {
                    query = query.Include(s => s.Users.Where(u => u.DeletedAt == null));
                }
            }

            if (includeTeams)
            {
                if (includeDeletedStepTeams)
                {
                    query = query.Include(s => s.Users)
                                .ThenInclude(u => u.Teams)
                                    .ThenInclude(ut => ut.Team);
                }
                else
                {
                    query = query.Include(s => s.Users)
                                .ThenInclude(u => u.Teams.Where(ut => ut.DeletedAt == null))
                                    .ThenInclude(ut => ut.Team);
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

        public async Task<(List<Step> Steps, int TotalCount)> GetAllStepsIncludeUsersAndTeamsQueriedAsync(
            Guid moderatorId,
            string? name,
            QueryParams? parameters)
        {
            IQueryable<Step> query = _context.Steps
                .Include(s => s.Users.Where(u => u.DeletedAt == null && u.Roles.Any(ur => ur.RoleId == moderatorId)))
                    .ThenInclude(u => u.Teams.Where(ut => ut.DeletedAt == null))
                        .ThenInclude(ut => ut.Team);

            query = query.Where(s => s.DeletedAt == null);

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(s => s.Name.ToUpper().Contains(name.ToUpper()));
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

        public async Task<(List<Step> Steps, int TotalCount)> GetAllStepsQueriedAsync(Guid moderatorId, string? name, QueryParams? parameters)
        {
            // 1. Pornim query simplu pe Steps
            IQueryable<Step> query = _context.Steps
                .Where(s => s.DeletedAt == null);

            // 2. Filtrare după nume dacă există
            if (!string.IsNullOrEmpty(name))
            {
                var upperName = name.ToUpper();
                query = query.Where(s => s.Name.ToUpper().Contains(upperName));
            }

            // 3. Total count înainte de paginare
            int totalCount = await query.CountAsync();

            // 4. Sortare
            if (!string.IsNullOrEmpty(parameters?.SortBy))
            {
                query = query.ApplySorting<Step>(parameters.SortBy, parameters.SortDescending ?? false);
            }

            // 5. Paginare
            if (parameters?.Page != null && parameters?.PageSize != null &&
                parameters.Page > 0 && parameters.PageSize > 0)
            {
                query = query
                    .Skip(parameters.PageSize.Value * (parameters.Page.Value - 1))
                    .Take(parameters.PageSize.Value);
            }

            // 6. Obținem doar Id-urile Steps filtrate + paginate
            var stepIds = await query.Select(s => s.Id).ToListAsync();
            if (!stepIds.Any())
                return (new List<Step>(), totalCount);

            // 7. Luăm Steps simple
            var steps = await _context.Steps
                .Where(s => stepIds.Contains(s.Id))
                .ToListAsync();

            // 8. Luăm Users pentru aceste Steps
            var users = await _context.Users
    .Where(u => stepIds.Contains(u.StepId))
    .Include(u => u.Teams)      // doar UserTeams, fără ThenInclude
    .ToListAsync();

            // 9. Grupăm Users pe StepId
            var usersGrouped = users.GroupBy(u => u.StepId)
                                    .ToDictionary(g => g.Key, g => g.ToList());

            // 10. Mapăm Users la Steps
            foreach (var step in steps)
            {
                step.Users = usersGrouped.TryGetValue(step.Id, out var stepUsers) ? stepUsers : new List<User>();
            }

            return (steps, totalCount);
        }
    }
}