using FlowManager.Application.Interfaces;
using FlowManager.Domain.IRepositories;
using FlowManager.Shared.DTOs;

namespace FlowManager.Application.Services
{
    public class AdminUsersService : IAdminUsersService
    {
        private readonly IAdminUsersRepository _adminUsersRepository;

        public AdminUsersService(IAdminUsersRepository adminUsersRepository)
        {
            _adminUsersRepository = adminUsersRepository;
        }

        public async Task<List<UserProfileDto>> GetUsersForImpersonationAsync(string? currentUserId, string? search, int page, int pageSize)
        {
            var users = await _adminUsersRepository.GetUsersForImpersonationAsync(currentUserId, search, page, pageSize);

            var result = new List<UserProfileDto>();

            foreach (var user in users)
            {
                var roles = user.Roles.Select(ur => ur.Role.Name).ToList();

                if (roles.Contains("Admin"))
                    continue;

                result.Add(new UserProfileDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email ?? "",
                    UserName = user.UserName,
                    Roles = roles
                });
            }

            return result;
        }
    }
}