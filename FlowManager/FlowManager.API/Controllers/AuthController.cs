using System.Security.Claims;
using FlowManager.Domain.Entities;
using FlowManager.Infrastructure.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FlowManager.Shared.DTOs.Requests.Auth;
using FlowManager.Shared.DTOs;
using FlowManager.Application.Interfaces;

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
        private readonly IUserService _userService;

        public AuthController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            RoleManager<Role> roleManager,
            AppDbContext context,
            IUserService userService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
            _userService = userService;
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

            try
            {
                List<string> userRole = await _userService.GetUserRolesByEmailAsync(request.Email);
                Console.WriteLine($"[DEBUG] User role: {userRole}");

                return Ok(new
                {
                    message = "Login successful",
                    roles = userRole
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] Error getting user role: {ex.Message}");
                return Ok(new
                {
                    message = "Login successful",
                    roles = new List<string>()
                });
            }
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
                Name = foundUser.Name ?? string.Empty,
                Email = foundUser.Email ?? string.Empty,
                UserName = foundUser.UserName,
                PhoneNumber = foundUser.PhoneNumber,
                StepId = foundUser.StepId,
                Roles = foundUser.Roles?
                    .Where(ur => ur.DeletedAt == null && ur.Role.DeletedAt == null)
                    .Select(ur => ur.Role.Name ?? string.Empty)
                    .ToList() ?? new List<string>()
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

            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return BadRequest("User with this email already exists");
            }

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

            var normalizedRole = request.Role.ToUpperInvariant();
            var roleEntity = await _roleManager.FindByNameAsync(normalizedRole);

            if (roleEntity == null)
            {   
                roleEntity = new Role
                {
                    Id = Guid.NewGuid(),
                    Name = normalizedRole,
                    NormalizedName = normalizedRole
                };
                await _roleManager.CreateAsync(roleEntity);
                Console.WriteLine($"[DEBUG] Created new role: {normalizedRole}");
            }

            var roleResult = await _userManager.AddToRoleAsync(user, normalizedRole);
            if (!roleResult.Succeeded)
            {
                var roleErrors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                Console.WriteLine($"[DEBUG] Role assignment failed: {roleErrors}");
                return BadRequest($"Role assignment failed: {roleErrors}");
            }

            Console.WriteLine($"[DEBUG] User {request.Email} registered successfully with role {normalizedRole}");
            return Ok(new { message = "User registered successfully" });
        }
    }
}