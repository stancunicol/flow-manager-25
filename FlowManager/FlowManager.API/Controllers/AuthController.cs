using System.Security.Claims;
using FlowManager.Application.DTOs;
using FlowManager.Application.DTOs.Requests.Auth;
using FlowManager.Domain.Entities;
using FlowManager.Infrastructure.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FlowManager.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly AppDbContext _context;

        public AuthController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            RoleManager<Role> roleManager,
            AppDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            Console.WriteLine($"[DEBUG] Login attempt - Email: {request.Email}");

            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Email and password are required");

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null || user.DeletedAt.HasValue)
            {
                Console.WriteLine("[DEBUG] User not found or deleted");
                return Unauthorized("Invalid credentials");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!result.Succeeded)
            {
                Console.WriteLine($"[DEBUG] Password check failed: {result}");
                return Unauthorized("Invalid credentials");
            }

            await _signInManager.SignInAsync(user, isPersistent: true);
            Console.WriteLine($"[DEBUG] User {request.Email} signed in successfully");

            return Ok(new { message = "Login successful" });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok(new { message = "Logged out successfully" });
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var user = HttpContext.User;

            if (user?.Identity == null || !user.Identity.IsAuthenticated)
            {
                return Unauthorized("User is not authenticated");
            }

            var email = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(email))
            {
                return BadRequest("User does not have an email claim");
            }

            var foundUser = await _context.Users
                .Include(u => u.Roles.Where(ur => ur.DeletedAt == null))
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == email && u.DeletedAt == null);

            if (foundUser == null)
            {
                return NotFound("User not found");
            }

            var dto = new UserProfileDto
            {
                Id = foundUser.Id,
                Name = foundUser.Name,
                Email = foundUser.Email,
                UserName = foundUser.UserName,
                Roles = foundUser.Roles
                    .Where(ur => ur.DeletedAt == null && ur.Role.DeletedAt == null)
                    .Select(ur => ur.Role.Name)
                    .ToList()
            };

            return Ok(dto);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            Console.WriteLine($"[DEBUG] Registration attempt - Name: {request.Name}, Email: {request.Email}, Role: {request.Role}");

            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Name, email, and password are required");
            }

            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return BadRequest("User with this email already exists");
            }

            // Create the user
            var user = new User
            {
                UserName = request.Email,
                Email = request.Email,
                Name = request.Name,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                Console.WriteLine($"[DEBUG] User creation failed: {errors}");
                return BadRequest($"User creation failed: {errors}");
            }

            // Ensure the role exists and add user to role
            var normalizedRole = request.Role.ToUpperInvariant();
            var roleEntity = await _roleManager.FindByNameAsync(normalizedRole);

            if (roleEntity == null)
            {
                roleEntity = new Role
                {
                    Name = normalizedRole,
                    NormalizedName = normalizedRole,
                    CreatedAt = DateTime.UtcNow
                };
                await _roleManager.CreateAsync(roleEntity);
                Console.WriteLine($"[DEBUG] Created new role: {normalizedRole}");
            }

            // Add user to role using UserRole entity
            var userRole = new UserRole
            {
                UserId = user.Id,
                RoleId = roleEntity.Id,
                CreatedAt = DateTime.UtcNow
            };

            _context.UserRoles.Add(userRole);
            await _context.SaveChangesAsync();

            Console.WriteLine($"[DEBUG] User {request.Email} registered successfully with role {normalizedRole}");
            return Ok(new { message = "User registered successfully" });
        }
    }
}