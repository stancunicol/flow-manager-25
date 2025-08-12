using FlowManager.Application.Interfaces;
using FlowManager.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;
using System.Text;

public class PasswordResetService : IPasswordResetService
{
    private readonly AppDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly IEmailService _emailService;

    public PasswordResetService(AppDbContext context, IMemoryCache cache, IEmailService emailService)
    {
        _context = context;
        _cache = cache;
        _emailService = emailService;
    }

    private static string GenerateRandomCode(int length = 6)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var bytes = new byte[length];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        var code = new StringBuilder(length);
        foreach (var b in bytes)
            code.Append(chars[b % chars.Length]);
        return code.ToString();
    }

    public async Task SendResetCodeAsync(string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) throw new Exception("User not found");

        var code = GenerateRandomCode(6);
        _cache.Set($"reset_{email}", code, TimeSpan.FromMinutes(15));

        var name = user.Name ?? "User";
        var subject = "Password Reset - Siemens FMST";
        var body = $@"Hello {name},

You have requested to reset your password.

Your verification code:

{code}

Important: This code is valid for 15 minutes.
Use this code on the reset password page to set your new password.

Best regards,
The Siemens FMST Team";

        await _emailService.SendEmailAsync(email, subject, body, false);
    }

    public Task<bool> VerifyResetCodeAsync(string email, string code)
    {
        if (_cache.TryGetValue($"reset_{email}", out string? cachedCode))
        {
            return Task.FromResult(cachedCode == code);
        }
        return Task.FromResult(false);
    }

    public async Task<bool> ResetPasswordAsync(string email, string code, string newPassword)
    {
        if (!_cache.TryGetValue($"reset_{email}", out string? cachedCode) || cachedCode != code)
            return false;

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) return false;

        user.PasswordHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(newPassword)));
        await _context.SaveChangesAsync();

        _cache.Remove($"reset_{email}");
        return true;
    }
}