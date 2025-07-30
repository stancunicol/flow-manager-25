using FlowManager.Domain.Entities;
using System.Net.Http.Json;

namespace FlowManager.Client.Services
{
    public class UserService
    {
        private readonly HttpClient _httpClient;

        public UserService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/users");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<User>>() ?? new List<User>();
            }
            catch
            {
                return new List<User>();
            }
        }

        public async Task<User?> GetUserAsync(Guid id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/users/{id}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<User>();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<User>> GetUsersByStepAsync(Guid stepId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/stepusers/step/{stepId}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<User>>() ?? new List<User>();
                }
                return new List<User>();
            }
            catch
            {
                return new List<User>();
            }
        }


        public async Task<bool> AssignUserToStepAsync(Guid stepId, Guid userId)
        {
            try
            {
                var response = await _httpClient.PostAsync($"api/stepusers/assign?stepId={stepId}&userId={userId}", null);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UnassignUserFromStepAsync(Guid stepId, Guid userId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/stepusers/unassign?stepId={stepId}&userId={userId}");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}