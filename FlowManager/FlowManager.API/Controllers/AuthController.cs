    using System.Security.Claims;
    using FlowManager.Application.Interfaces;
    using FlowManager.Domain.Entities;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    namespace FlowManager.API.Controllers
    {
        //[ApiController]
        //[Route("api/auth")]
        //public class AuthController:ControllerBase
        //{
        //    private readonly IAuthService _authService;

        //    public AuthController(IAuthService authService)
        //    {
        //        _authService = authService;
        //    }

        //    //[HttpPost("login")]
        //    //public async Task<IActionResult>Login([FromForm]string email, [FromForm]string password)
        //    //{
        //    //    var success=await _authService.Login(email, password);
        //    //    return success ? Ok("Login successful") : Unauthorized("Invalid credentials");
        //    //}

        //    [HttpPost("login")]
        //    public async Task<IActionResult> Login([FromBody] Dictionary<string, string> body)
        //    {
        //        var email = body.GetValueOrDefault("email");
        //        var password = body.GetValueOrDefault("password");

        //        Console.WriteLine($"[DEBUG] Email: {email}, Password: {password}");

        //        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        //            return BadRequest("Email and password are required");

        //        var success = await _authService.Login(email, password);
        //        return success ? Ok("Login successful") : Unauthorized("Invalid credentials");
        //    }

        //    [HttpPost("logout")]
        //    public async Task<IActionResult>Logout()
        //    {
        //        await _authService.Logout();    
        //        return Ok("Logged out");
        //    }
        //    [Authorize]
        //    [HttpGet("me")]
        //    public async Task<IActionResult> GetCurrentUser()
        //    {
        //        var user = HttpContext.User;

        //        if (user?.Identity == null || !user.Identity.IsAuthenticated)
        //        {
        //            return Unauthorized("User is not authenticated");
        //        }

        //        var email = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

        //        if (string.IsNullOrEmpty(email))
        //        {
        //            return BadRequest("User does not have an email claim");
        //        }

        //        var foundUser = await _authService.GetUserByEmail(email);

        //        if (foundUser == null)
        //        {
        //            return NotFound("User not found");
        //        }

        //        var dto = new UserProfileDto
        //        {
        //            Id = foundUser.Id,
        //            Name = foundUser.Name,
        //            Email = foundUser.Email,
        //            UserName = foundUser.UserName,
        //            UserRoles = foundUser.UserRoles?.Select(ur => ur.Role.Name).ToList() ?? new List<string>()
        //        };

        //        return Ok(dto); 
        //    }

        //    [HttpPost("register")]
        //    public async Task<IActionResult> Register([FromBody] Dictionary<string, string> body)
        //    {
        //        var name = body.GetValueOrDefault("name");
        //        var email = body.GetValueOrDefault("email");
        //        var password = body.GetValueOrDefault("password");
        //        var role = body.GetValueOrDefault("role", "basic"); // Default to basic if not provided

        //        Console.WriteLine($"[DEBUG] Registration attempt - Name: {name}, Email: {email}, Role: {role}");

        //        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        //        {
        //            return BadRequest("Name, email, and password are required");
        //        }

        //        var (success, message) = await _authService.Register(name, email, password, role);
                
        //        if (success)
        //        {
        //            return Ok(new { message });
        //        }
        //        else
        //        {
        //            return BadRequest(new { message });
        //        }
        //    }

        
    }
