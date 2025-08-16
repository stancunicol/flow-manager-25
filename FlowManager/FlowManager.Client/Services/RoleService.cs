using FlowManager.Client.DTOs;
using FlowManager.Shared.DTOs.Responses.Role;
using System.Net.Http.Json;

namespace FlowManager.Client.Services
{
    public class RoleService
    {
        private readonly HttpClient _httpClient;

        public RoleService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ApiResponse<List<RoleResponseDto>>> GetAllRolesAsync()
        {
            HttpResponseMessage response = await _httpClient.GetAsync("api/roles/");

            return await response.Content.ReadFromJsonAsync<ApiResponse<List<RoleResponseDto>>>() ?? new ApiResponse<List<RoleResponseDto>>();
        }
    }
}
