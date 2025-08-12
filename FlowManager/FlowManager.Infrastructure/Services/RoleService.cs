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
    public class RoleService : IRoleService
    {
        private readonly AppDbContext _dbContext;

        public RoleService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Role>?> GetAllRolesAsync()
        {
            return await _dbContext.Roles.ToListAsync();
        }

        public async Task<Role?> GetRoleByIdAsync(Guid id)
        {
            return await _dbContext.Roles.FirstOrDefaultAsync(r => r.Id == id); 
        }
    }
}
