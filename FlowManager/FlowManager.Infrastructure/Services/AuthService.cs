using FlowManager.Application.Interfaces;
using FlowManager.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FlowManager.Infrastructure;

namespace FlowManager.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly AppDbContext _context;

        public AuthService(SignInManager<User> signInManager, UserManager<User> userManager, AppDbContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
        }

        public async Task<bool> Login(string email, string password)
        {
            Console.WriteLine($"[AuthService] Attempting login for email: {email}");
            
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) 
            {
                Console.WriteLine("[AuthService] User not found");
                return false;
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);
            if (!result.Succeeded) 
            {
                Console.WriteLine($"[AuthService] Password check failed: {result}");
                return false;
            }

            await _signInManager.SignInAsync(user, isPersistent: true);
            Console.WriteLine($"[AuthService] User {email} signed in successfully with persistent cookie");
            return true;
        }



        public async Task Logout()
        {
            await _signInManager.SignOutAsync();
        }
        
        public async Task<User?> GetUserByEmail(string email)
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Include(u => u.Forms)
                .Include(u => u.StepUsers)
                .Include(u => u.UpdateHistories)
                .FirstOrDefaultAsync(u => u.Email == email);
        }
    }
}
