using FlowManager.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;
using System.Text;

namespace FlowManager.Application.Services
{
    public class PasswordResetService : IPasswordResetService
    {
        private readonly IUserService _userService;
        private readonly IMemoryCache _cache;
        private readonly IEmailService _emailService;

        public PasswordResetService(IUserService userService, IMemoryCache cache, IEmailService emailService)
        {
            _userService = userService;
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
            var user = await _userService.GetUserByEmailAsync(email);
            if (user == null) throw new Exception("User not found");

            var code = GenerateRandomCode(6);
            _cache.Set($"reset_{email}", code, TimeSpan.FromMinutes(15));

            var name = user.Name ?? "User";
            await _emailService.SendPasswordResetCodeAsync(email, name, code);
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

            var user = await _userService.GetUserByEmailAsync(email);
            if (user == null) return false;

            await _userService.ResetPassword(user.Id, newPassword);

            _cache.Remove($"reset_{email}");
            return true;
        }
    }
}