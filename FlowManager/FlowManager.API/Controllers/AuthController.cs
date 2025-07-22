using FlowManager.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FlowManager.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController:ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        //[HttpPost("login")]
        //public async Task<IActionResult>Login([FromForm]string email, [FromForm]string password)
        //{
        //    var success=await _authService.Login(email, password);
        //    return success ? Ok("Login successful") : Unauthorized("Invalid credentials");
        //}

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Dictionary<string, string> body)
        {
            var email = body.GetValueOrDefault("email");
            var password = body.GetValueOrDefault("password");

            Console.WriteLine($"[DEBUG] Email: {email}, Password: {password}");

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return BadRequest("Email and password are required");

            var success = await _authService.Login(email, password);
            return success ? Ok("Login successful") : Unauthorized("Invalid credentials");
        }

        [HttpPost("logout")]
        public async Task<IActionResult>Logout()
        {
            await _authService.Logout();
            return Ok("Logged out");
        }

    }
}
