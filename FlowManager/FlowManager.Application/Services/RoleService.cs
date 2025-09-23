using FlowManager.Application.Interfaces;
using FlowManager.Domain.Entities;
using FlowManager.Domain.Exceptions;
using FlowManager.Domain.IRepositories;
using FlowManager.Shared.DTOs.Responses.Role;

namespace FlowManager.Infrastructure.Services
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;

        public RoleService(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        public async Task<List<RoleResponseDto>> GetAllRolesAsync()
        {
            var result = await _roleRepository.GetAllRolesAsync();

            if(result == null || !result.Any())
            {
                throw new EntryNotFoundException("No roles found.");
            }

            return result.Select(r => new RoleResponseDto
            {
                Id = r.Id,
                Name = r.Name,
            }).ToList();
        }

        public async Task<RoleResponseDto> GetRoleByIdAsync(Guid id)
        {
            Role? result = await _roleRepository.GetRoleByIdAsync(id);

            if(result == null)
            {
                 throw new EntryNotFoundException($"Role with id {id} was not found.");
            }

            return new RoleResponseDto
            {
                Id = result.Id,
                Name = result.Name,
            };
        }
    }
}
