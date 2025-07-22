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

        [HttpPost("login")]
        public async Task<IActionResult>Login([FromForm]string email, [FromForm]string password)
        {
            var success=await _authService.Login(email, password);
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
