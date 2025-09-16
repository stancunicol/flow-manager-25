using FlowManager.Domain.Dtos;
using FlowManager.Domain.Entities;
using FlowManager.Domain.IRepositories;
using FlowManager.Infrastructure.Context;
using FlowManager.Infrastructure.Utils;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Infrastructure.Repositories
{
    public class FlowRepository : IFlowRepository
    {
        private readonly AppDbContext _context;

        public FlowRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Flow>> GetAllFlowsAsync()
        {
            return await _context.Flows
                .Include(f => f.Steps)
                    .ThenInclude(fs => fs.FlowStepItems)
                .Include(f => f.FormTemplateFlows.Where(ft => ft.DeletedAt == null && ft.FormTemplate.DeletedAt == null))
                    .ThenInclude(formTemplateFlow => formTemplateFlow.FormTemplate)
                .ToListAsync();
        }

        public async Task<Flow?> GetFlowByIdAsync(Guid id)
        {
            return await _context.Flows
                .Include(f => f.Steps.Where(fs => fs.DeletedAt == null))
                    .ThenInclude(fs => fs.FlowStepItems)
                        .ThenInclude(flowStepItem => flowStepItem.Step)
                .Include(f => f.FormTemplateFlows.Where(ft => ft.DeletedAt == null && ft.FormTemplate.DeletedAt == null))
                    .ThenInclude(formTemplateFlow => formTemplateFlow.FormTemplate)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<Flow> CreateFlowAsync(Flow flow)
        {
            _context.Flows.Add(flow);
            await _context.SaveChangesAsync();
            return flow;
        }

        public async Task<(List<Flow> Data, int TotalCount)> GetAllFlowsQueriedAsync(string? globalSearchTerm, QueryParams? parameters)
        {
            IQueryable<Flow> query = _context.Flows
                .Include(f => f.Steps)
                    .ThenInclude(fs => fs.FlowStepItems)
                        .ThenInclude(flowStepItem => flowStepItem.Step)
                .Include(f => f.FormTemplateFlows.Where(ft => ft.DeletedAt == null && ft.FormTemplate.DeletedAt == null))
                    .ThenInclude(formTemplateFlow => formTemplateFlow.FormTemplate);

            // filtering
            if (!string.IsNullOrEmpty(globalSearchTerm))
            {
                query = query.Where(f => f.Name.ToUpper().Contains(globalSearchTerm.ToUpper()));
            }

            int totalCount = await query.CountAsync();

            if (parameters == null)
            {
                return (await query.ToListAsync(), totalCount);
            }

            // sorting
            if (!string.IsNullOrEmpty(parameters.SortBy))
            {
                if (parameters.SortDescending is bool sortDesc)
                {
                    query = query.ApplySorting<Flow>(parameters.SortBy, sortDesc);
                }
                else
                {
                    query = query.ApplySorting<Flow>(parameters.SortBy, false);
                }
            }

            // pagination
            if (parameters.Page == null || parameters.Page < 0 ||
                parameters.PageSize == null || parameters.PageSize < 0)
            {
                return (await query.ToListAsync(), totalCount);
            }
            else
            {
                return (await query
                    .Skip((parameters.Page.Value - 1) * parameters.PageSize.Value)
                    .Take(parameters.PageSize.Value)
                    .ToListAsync(), totalCount);
            }
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<Flow?> GetFlowIncludeDeletedStepsByIdAsync(Guid id)
        {
            return await _context.Flows
                .Include(f => f.Steps.Where(s => s.DeletedAt == null))
                    .ThenInclude(fs => fs.FlowStepItems)
                .Include(f => f.FormTemplateFlows.Where(ft => ft.DeletedAt == null && ft.FormTemplate.DeletedAt == null))
                    .ThenInclude(formTemplateFlow => formTemplateFlow.FormTemplate)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<Flow?> GetFlowByIdIncludeStepsAsync(Guid flowId, Guid moderatorRoleId)
        {
            return await _context.Flows
                .Where(f => f.Id == flowId && f.DeletedAt == null)
                .Where(f => f.Steps.Any(fs => fs.DeletedAt == null && (
                    fs.AssignedUsers.Any(fsu => fsu.DeletedAt == null &&
                        fsu.User.Roles.Any(ur => ur.RoleId == moderatorRoleId)) ||
                    fs.AssignedTeams.Any(fst => fst.DeletedAt == null &&
                        fst.Team.Users.Any(ut => ut.DeletedAt == null &&
                            ut.User.Roles.Any(ur => ur.RoleId == moderatorRoleId)))
                )))
                .Include(f => f.Steps.Where(fs => fs.DeletedAt == null))
                    .ThenInclude(fs => fs.FlowStepItems)
                        .ThenInclude(flowStepItems => flowStepItems.Step)
                .Include(f => f.Steps.Where(fs => fs.DeletedAt == null))
                    .ThenInclude(fs => fs.AssignedUsers.Where(fsu => fsu.DeletedAt == null &&
                        fsu.User.Roles.Any(ur => ur.RoleId == moderatorRoleId)))
                        .ThenInclude(fsu => fsu.User)
                            .ThenInclude(u => u.Roles)
                .Include(f => f.Steps.Where(fs => fs.DeletedAt == null))
                    .ThenInclude(fs => fs.AssignedUsers.Where(fsu => fsu.DeletedAt == null &&
                        fsu.User.Roles.Any(ur => ur.RoleId == moderatorRoleId)))
                        .ThenInclude(fsu => fsu.User)
                            .ThenInclude(u => u.Teams.Where(ut => ut.DeletedAt == null))
                                .ThenInclude(ut => ut.Team)
                .Include(f => f.Steps.Where(fs => fs.DeletedAt == null))
                    .ThenInclude(fs => fs.AssignedTeams.Where(fst => fst.DeletedAt == null &&
                        fst.Team.Users.Any(ut => ut.DeletedAt == null &&
                            ut.User.Roles.Any(ur => ur.RoleId == moderatorRoleId))))
                        .ThenInclude(fst => fst.Team)
                            .ThenInclude(t => t.Users.Where(ut => ut.DeletedAt == null &&
                                ut.User.Roles.Any(ur => ur.RoleId == moderatorRoleId)))
                                .ThenInclude(ut => ut.User)
                                    .ThenInclude(u => u.Roles)
                .FirstOrDefaultAsync();
        }

        public Task<Flow?> GetFlowByNameAsync(string flowName)
        {
            return _context.Flows
                .FirstOrDefaultAsync(f => f.Name == flowName);
        }

        public async Task<Flow?> GetFlowByFormTemplateIdAsync(Guid formTemplateId)
        {
            var flows = await _context.Flows
                .Where(f => f.DeletedAt == null)
                .Include(f => f.Steps.Where(fs => fs.DeletedAt == null))
                    .ThenInclude(fs => fs.FlowStepItems)
                        .ThenInclude(flowStepItem => flowStepItem.Step)
                .Include(f => f.FormTemplateFlows.Where(ftf => ftf.DeletedAt == null))
                    .ThenInclude(ftf => ftf.FormTemplate)
                .ToListAsync();

            return flows.FirstOrDefault(f => f.ActiveFormTemplateId == formTemplateId);
        }
    }
}