using Microsoft.AspNetCore.Mvc;
using FlowManager.Application.Interfaces;
using FlowManager.Application.DTOs;

namespace FlowManager.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PasswordResetController : ControllerBase
    {
        private readonly IPasswordResetService _passwordResetService;
        private readonly ILogger<PasswordResetController> _logger;

        public PasswordResetController(IPasswordResetService passwordResetService, ILogger<PasswordResetController> logger)
        {
            _passwordResetService = passwordResetService ?? throw new ArgumentNullException(nameof(passwordResetService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost("request")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RequestResetAsync([FromBody] string email)
        {
            await _passwordResetService.SendResetCodeAsync(email);

            return Ok(new
            {
                Result = "Reset code sent",
                Success = true,
                Message = "Password reset code sent successfully.",
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpPost("verify")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> VerifyCodeAsync([FromBody] VerifyCodeDto dto)
        {
            var result = await _passwordResetService.VerifyResetCodeAsync(dto.Email, dto.Code);

            return Ok(new
            {
                Result = result,
                Success = result,
                Message = result ? "Code verified successfully." : "Invalid verification code.",
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpPost("confirm")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ConfirmResetAsync([FromBody] ConfirmResetDto dto)
        {
            var result = await _passwordResetService.ResetPasswordAsync(dto.Email, dto.Code, dto.NewPassword);

            return Ok(new
            {
                Result = result,
                Success = result,
                Message = result ? "Password reset successfully." : "Invalid code or password reset failed.",
                Timestamp = DateTime.UtcNow
            });
        }
    }
}