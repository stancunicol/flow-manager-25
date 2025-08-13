using FlowManager.Application.DTOs.Responses.Role;
using FlowManager.Application.Interfaces;
using FlowManager.Domain.Entities;
using FlowManager.Domain.IRepositories;
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
        private readonly IRoleRepository _roleRepository;

        public RoleService(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        public async Task<List<RoleResponseDto>?> GetAllRolesAsync()
        {
            var result = await _roleRepository.GetAllRolesAsync();

            if(result == null || !result.Any())
            {
                return null; // middleware exception
            }

            return result.Select(r => new RoleResponseDto
            {
                Id = r.Id,
                Name = r.Name,
            }).ToList();
        }

        public async Task<RoleResponseDto?> GetRoleByIdAsync(Guid id)
        {
            var result = await _roleRepository.GetRoleByIdAsync(id);
            return new RoleResponseDto
            {
                Id = result.Id,
                Name = result.Name,
            };
        }
    }
}
