using FlowManager.Shared.DTOs;
using System.Net.Http.Json;

namespace FlowManager.Client.Services
{
    public class ClientAuthService
    {
        private readonly HttpClient _httpClient;

        public ClientAuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> RequestPasswordResetAsync(string email)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/passwordreset/request", email);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ConfirmPasswordResetAsync(string email, string code, string newPassword)
        {
            try
            {
                var request = new ConfirmResetDto
                {
                    Email = email,
                    Code = code,
                    NewPassword = newPassword
                };
                var response = await _httpClient.PostAsJsonAsync("api/passwordreset/confirm", request);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}