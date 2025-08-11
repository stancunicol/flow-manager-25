using FlowManager.Application.Interfaces;
using FlowManager.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FlowManager.Infrastructure;

namespace FlowManager.Infrastructure.Services
{
    //public class AuthService : IAuthService
    //{
    //    private readonly SignInManager<User> _signInManager;
    //    private readonly UserManager<User> _userManager;
    //    private readonly AppDbContext _context;
    //    private readonly RoleManager<IdentityRole<Guid>> _roleManager;

    //    public AuthService(SignInManager<User> signInManager, UserManager<User> userManager, AppDbContext context, RoleManager<IdentityRole<Guid>> roleManager)
    //    {
    //        _signInManager = signInManager;
    //        _userManager = userManager;
    //        _context = context;
    //        _roleManager = roleManager;
    //    }

    //    public async Task<bool> Login(string email, string password)
    //    {
    //        Console.WriteLine($"[AuthService] Attempting login for email: {email}");
            
    //        var user = await _userManager.FindByEmailAsync(email);
    //        if (user == null) 
    //        {
    //            Console.WriteLine("[AuthService] User not found");
    //            return false;
    //        }

    //        var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);
    //        if (!result.Succeeded) 
    //        {
    //            Console.WriteLine($"[AuthService] Password check failed: {result}");
    //            return false;
    //        }

    //        await _signInManager.SignInAsync(user, isPersistent: true);
    //        Console.WriteLine($"[AuthService] User {email} signed in successfully with persistent cookie");
    //        return true;
    //    }

    //    public async Task Logout()
    //    {
    //        await _signInManager.SignOutAsync();
    //    }
        
    //    public async Task<User?> GetUserByEmail(string email)
    //    {
    //        return await _context.Users
    //            .Include(u => u.UserRoles)
    //                .ThenInclude(ur => ur.Role)
    //            .Include(u => u.Forms)
    //            .Include(u => u.StepUsers)
    //            .Include(u => u.UpdateHistories)
    //            .FirstOrDefaultAsync(u => u.Email == email);
    //    }

    //    public async Task<(bool Success, string Message)> Register(string name, string email, string password, string role)
    //    {
    //        Console.WriteLine($"[AuthService] Attempting registration for email: {email}, role: {role}");

    //        // Check if user already exists
    //        var existingUser = await _userManager.FindByEmailAsync(email);
    //        if (existingUser != null)
    //        {
    //            return (false, "User with this email already exists");
    //        }

    //        // Create the user
    //        var user = new User
    //        {
    //            UserName = email,
    //            Email = email,
    //            Name = name,
    //            EmailConfirmed = true // For simplicity, assume email is confirmed
    //        };

    //        var result = await _userManager.CreateAsync(user, password);
    //        if (!result.Succeeded)
    //        {
    //            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
    //            Console.WriteLine($"[AuthService] User creation failed: {errors}");
    //            return (false, $"User creation failed: {errors}");
    //        }

    //        // Ensure the role exists
    //        var normalizedRole = role.ToUpperInvariant();
    //        if (!await _roleManager.RoleExistsAsync(normalizedRole))
    //        {
    //            await _roleManager.CreateAsync(new IdentityRole<Guid>(normalizedRole));
    //            Console.WriteLine($"[AuthService] Created new role: {normalizedRole}");
    //        }

    //        // Add user to role
    //        var roleResult = await _userManager.AddToRoleAsync(user, normalizedRole);
    //        if (!roleResult.Succeeded)
    //        {
    //            var roleErrors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
    //            Console.WriteLine($"[AuthService] Role assignment failed: {roleErrors}");
    //            return (false, $"Role assignment failed: {roleErrors}");
    //        }

    //        Console.WriteLine($"[AuthService] User {email} registered successfully with role {normalizedRole}");
    //        return (true, "User registered successfully");
    //    }
    //}
}
