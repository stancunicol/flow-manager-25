using FlowManager.Domain.Entities;
using System.Net.Http.Json;

namespace FlowManager.Client.Services
{
    public class FlowService
    {
        private readonly HttpClient _httpClient;

        public FlowService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Flow>> GetFlowsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/flows");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<Flow>>() ?? new List<Flow>();
            }
            catch
            {
                return new List<Flow>();
            }
        }

        public async Task<Flow?> GetFlowAsync(Guid id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/flows/{id}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<Flow>();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<Flow?> CreateFlowAsync(Flow flow)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/flows", flow);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<Flow>();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> UpdateFlowAsync(Guid id, Flow flow)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/flows/{id}", flow);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteFlowAsync(Guid id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/flows/{id}");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<Step>> GetFlowStepsAsync(Guid flowId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/flows/{flowId}/steps");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<Step>>() ?? new List<Step>();
                }
                return new List<Step>();
            }
            catch
            {
                return new List<Step>();
            }
        }
    }
}