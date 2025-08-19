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

        // FIX: Include TOATE entities pentru PATCH operations (inclusiv deleted)
        public async Task<Step?> GetStepByIdAsync(Guid id)
        {
            return await _context.Steps
                .Where(s => s.DeletedAt == null)
                .Include(s => s.Users) // TOATE - nu filtra pe DeletedAt aici
                    .ThenInclude(su => su.User)
                .Include(s => s.Teams) // TOATE - nu filtra pe DeletedAt aici
                    .ThenInclude(st => st.Team)
                .Include(s => s.FlowSteps)
                    .ThenInclude(fs => fs.Flow)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        // Metodă separată pentru display (cu filtrare)
        public async Task<Step?> GetStepByIdForDisplayAsync(Guid id)
        {
            return await _context.Steps
                .Where(s => s.DeletedAt == null)
                .Include(s => s.Users.Where(su => su.DeletedAt == null))
                    .ThenInclude(su => su.User)
                .Include(s => s.Teams.Where(st => st.DeletedAt == null))
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
            await SaveChangesWithRetryAsync();
            return step;
        }

        public async Task<Step> DeleteStepAsync(Step step)
        {
            step.DeletedAt = DateTime.UtcNow;
            step.UpdatedAt = DateTime.UtcNow;
            await SaveChangesWithRetryAsync();
            return step;
        }

        // FIX: Implementare SaveChanges cu retry logic
        public async Task SaveChangesAsync()
        {
            await SaveChangesWithRetryAsync();
        }

        public async Task SaveChangesWithRetryAsync()
        {
            const int maxRetries = 3;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return; // Success
                }
                catch (DbUpdateConcurrencyException ex) when (attempt < maxRetries)
                {
                    // Reload toate entities care au probleme
                    foreach (var entry in ex.Entries)
                    {
                        await entry.ReloadAsync();

                        // Reload collections pentru Step entities
                        if (entry.Entity is Step step)
                        {
                            await entry.Collection(nameof(step.Users)).LoadAsync();
                            await entry.Collection(nameof(step.Teams)).LoadAsync();
                            await entry.Collection(nameof(step.FlowSteps)).LoadAsync();
                        }
                    }

                    // Wait a bit before retry
                    await Task.Delay(50 * attempt);
                }
            }
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


        // ==========================================
        // METODE PENTRU MANAGEMENTUL RELAȚIILOR (evită concurența)
        // ==========================================

        /// <summary>
        /// Înlocuiește userii unui step - operațiune atomică
        /// </summary>
        public async Task<Step> ReplaceStepUsersAsync(Guid stepId, List<Guid> newUserIds)
        {
            var step = await GetStepByIdSimpleAsync(stepId);
            if (step == null)
                throw new ArgumentException($"Step {stepId} not found");

            // 1. Marchează ca șterse doar userii care nu sunt în lista nouă
            var currentUserIds = await _context.StepUsers
                .Where(su => su.StepId == stepId && su.DeletedAt == null)
                .Select(su => su.UserId)
                .ToListAsync();

            var usersToDelete = currentUserIds.Except(newUserIds).ToList();

            if (usersToDelete.Any())
            {
                await _context.StepUsers
                    .Where(su => su.StepId == stepId && usersToDelete.Contains(su.UserId) && su.DeletedAt == null)
                    .ExecuteUpdateAsync(su => su
                        .SetProperty(p => p.DeletedAt, DateTime.UtcNow)
                        .SetProperty(p => p.UpdatedAt, DateTime.UtcNow));
            }

            // 2. Adaugă sau restaurează userii noi
            foreach (var userId in newUserIds)
            {
                if (!currentUserIds.Contains(userId))
                {
                    // Verifică dacă există o relație ștearsă
                    var deletedRelation = await _context.StepUsers
                        .FirstOrDefaultAsync(su => su.StepId == stepId && su.UserId == userId && su.DeletedAt != null);

                    if (deletedRelation != null)
                    {
                        // Restaurează relația existentă
                        deletedRelation.DeletedAt = null;
                        deletedRelation.UpdatedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        // Creează relație nouă
                        _context.StepUsers.Add(new StepUser
                        {
                            StepId = stepId,
                            UserId = userId
                        });
                    }
                }
            }

            await SaveChangesWithRetryAsync();

            // Returnează step-ul cu relațiile încărcate
            return await GetStepByIdForDisplayAsync(stepId) ?? step;
        }

        /// <summary>
        /// Înlocuiește teams unui step - operațiune atomică
        /// </summary>
        public async Task<Step> ReplaceStepTeamsAsync(Guid stepId, List<Guid> newTeamIds)
        {
            var step = await GetStepByIdSimpleAsync(stepId);
            if (step == null)
                throw new ArgumentException($"Step {stepId} not found");

            // 1. Marchează ca șterse doar teams care nu sunt în lista nouă
            var currentTeamIds = await _context.StepTeams
                .Where(st => st.StepId == stepId && st.DeletedAt == null)
                .Select(st => st.TeamId)
                .ToListAsync();

            var teamsToDelete = currentTeamIds.Except(newTeamIds).ToList();

            if (teamsToDelete.Any())
            {
                await _context.StepTeams
                    .Where(st => st.StepId == stepId && teamsToDelete.Contains(st.TeamId) && st.DeletedAt == null)
                    .ExecuteUpdateAsync(st => st
                        .SetProperty(p => p.DeletedAt, DateTime.UtcNow)
                        .SetProperty(p => p.UpdatedAt, DateTime.UtcNow));
            }

            // 2. Adaugă sau restaurează teams noi
            foreach (var teamId in newTeamIds)
            {
                if (!currentTeamIds.Contains(teamId))
                {
                    // Verifică dacă există o relație ștearsă
                    var deletedRelation = await _context.StepTeams
                        .FirstOrDefaultAsync(st => st.StepId == stepId && st.TeamId == teamId && st.DeletedAt != null);

                    if (deletedRelation != null)
                    {
                        // Restaurează relația existentă
                        deletedRelation.DeletedAt = null;
                        deletedRelation.UpdatedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        // Creează relație nouă
                        _context.StepTeams.Add(new StepTeam
                        {
                            StepId = stepId,
                            TeamId = teamId
                        });
                    }
                }
            }

            await SaveChangesWithRetryAsync();

            // Returnează step-ul cu relațiile încărcate
            return await GetStepByIdForDisplayAsync(stepId) ?? step;
        }

        /// <summary>
        /// Actualizează doar numele unui step
        /// </summary>
        public async Task<Step> UpdateStepNameAsync(Guid stepId, string newName)
        {
            await _context.Steps
                .Where(s => s.Id == stepId && s.DeletedAt == null)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.Name, newName)
                    .SetProperty(p => p.UpdatedAt, DateTime.UtcNow));

            return await GetStepByIdForDisplayAsync(stepId)
                ?? throw new ArgumentException($"Step {stepId} not found");
        }

        /// <summary>
        /// Verifică dacă userii există - pentru validare în service
        /// </summary>
        public async Task<List<Guid>> ValidateUsersExistAsync(List<Guid> userIds)
        {
            var existingIds = await _context.Users
                .Where(u => userIds.Contains(u.Id) && u.DeletedAt == null)
                .Select(u => u.Id)
                .ToListAsync();

            return userIds.Except(existingIds).ToList(); // returnează ID-urile care NU există
        }

        /// <summary>
        /// Verifică dacă teams există - pentru validare în service
        /// </summary>
        public async Task<List<Guid>> ValidateTeamsExistAsync(List<Guid> teamIds)
        {
            var existingIds = await _context.Teams
                .Where(t => teamIds.Contains(t.Id) && t.DeletedAt == null)
                .Select(t => t.Id)
                .ToListAsync();

            return teamIds.Except(existingIds).ToList(); // returnează ID-urile care NU există
        }

        /// <summary>
        /// Versiune optimizată pentru PATCH - încarcă minimal
        /// </summary>
        public async Task<Step?> GetStepByIdForPatchAsync(Guid id)
        {
            return await _context.Steps
                .Where(s => s.DeletedAt == null)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

    }


}