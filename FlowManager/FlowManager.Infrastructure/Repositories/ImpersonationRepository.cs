using FlowManager.Domain.Entities;
using FlowManager.Domain.IRepositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FlowManager.Infrastructure.Repositories
{
    public class ImpersonationRepository : IImpersonationRepository
    {
        private readonly UserManager<User> _userManager;

        public ImpersonationRepository(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<User?> FindUserByIdAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        public async Task<User?> FindUserByIdWithStepAsync(Guid userId)
        {
            return await _userManager.Users
                .Include(u => u.Step)
                .FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null);
        }

        public async Task<IList<string>> GetUserRolesAsync(User user)
        {
            return await _userManager.GetRolesAsync(user);
        }
    }
}