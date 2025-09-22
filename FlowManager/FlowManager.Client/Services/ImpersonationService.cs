using FlowManager.Client.DTOs;
using FlowManager.Shared.DTOs;
using FlowManager.Shared.DTOs.Requests.Impersonation;
using FlowManager.Shared.DTOs.Responses.Impersonation;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using System.Text.Json;

namespace FlowManager.Client.Services
{
    public class ImpersonationService
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;

        public ImpersonationService(HttpClient httpClient, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
        }

        public async Task<ApiResponse<bool>> StartImpersonationAsync(Guid userId, string reason = "")
        {
            try
            {
                var request = new StartImpersonationRequestDto
                {
                    UserId = userId,
                    Reason = reason
                };

                var response = await _httpClient.PostAsJsonAsync("api/admin/impersonation/start", request);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<ImpersonationResponseDto>>();

                    if (result?.Success == true)
                    {
                        return new ApiResponse<bool> { Success = true, Result = true, Message = result.Message };
                    }
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = $"Failed to start impersonation: {errorContent}"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<bool>> EndImpersonationAsync()
        {
            try
            {
                var response = await _httpClient.PostAsync("api/admin/impersonation/end", null);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
                    return new ApiResponse<bool> 
                    { 
                        Success = true, 
                        Result = true, 
                        Message = result?.Message ?? "Impersonation ended successfully"
                    };
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = $"Failed to end impersonation: {errorContent}"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<bool> IsImpersonating()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/admin/impersonation/status");
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
                    return result?.Success == true && result.Result == true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string?> GetOriginalAdminName()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/admin/impersonation/original-admin");
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<string>>();
                    return result?.Result;
                }
                return null;
            }
            catch 
            { 
                return null;
            }
        }

        public async Task<string?> GetImpersonatedUserName()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/admin/impersonation/current-user");
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<string>>();
                    return result?.Result;
                }
                return null;
            }
            catch 
            { 
                return null;
            }
        }
    }

    public class ImpersonationInfo
    {
        public bool IsImpersonating { get; set; }
        public string ImpersonatedUserName { get; set; } = "";
        public string OriginalAdminName { get; set; } = "";
        public string SessionId { get; set; } = "";
    }

    

   
}