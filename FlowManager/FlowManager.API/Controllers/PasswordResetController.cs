using Microsoft.AspNetCore.Mvc;
using FlowManager.Application.Interfaces;
using FlowManager.Application.DTOs;

[ApiController]
[Route("api/[controller]")]
public class PasswordResetController : ControllerBase
{
    private readonly IPasswordResetService _service;

    public PasswordResetController(IPasswordResetService service)
    {
        _service = service;
    }

    [HttpPost("request")]
    public async Task<IActionResult> RequestReset([FromBody] string email)
    {
        await _service.SendResetCodeAsync(email);
        return Ok();
    }

    [HttpPost("verify")]
    public async Task<IActionResult> VerifyCode([FromBody] VerifyCodeDto dto)
    {
        var valid = await _service.VerifyResetCodeAsync(dto.Email, dto.Code);
        return valid ? Ok() : BadRequest("Invalid code");
    }

    [HttpPost("confirm")]
    public async Task<IActionResult> ConfirmReset([FromBody] ConfirmResetDto dto)
    {
        var result = await _service.ResetPasswordAsync(dto.Email, dto.Code, dto.NewPassword);
        return result ? Ok() : BadRequest("Invalid code or password");
    }
}