using FlowManager.Client.DTOs;
using FlowManager.Domain.Entities;
using FlowManager.Shared.DTOs.Requests.User;
using FlowManager.Shared.DTOs.Responses;
using FlowManager.Shared.DTOs.Responses.User;
using System.Net.Http.Json;
using System.Web;
using static System.Net.WebRequestMethods;

namespace FlowManager.Client.Services
{
    public class UserService
    {
        private readonly HttpClient _httpClient;

        public UserService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ApiResponse<PagedResponseDto<UserResponseDto>>> GetAllUsersQueriedAsync(QueriedUserRequestDto? payload = null)
        {
            try
            {
                var uriBuilder = new UriBuilder(_httpClient.BaseAddress!)
                {
                    Path = "api/users/queried"
                };

                var query = HttpUtility.ParseQueryString(string.Empty);

                if (payload != null)
                {
                    if (!string.IsNullOrEmpty(payload.Email))
                        query["Email"] = payload.Email;

                    if (payload.QueryParams != null)
                    {
                        var qp = payload.QueryParams;

                        if (qp.Page.HasValue)
                            query["QueryParams.Page"] = qp.Page.Value.ToString();

                        if (qp.PageSize.HasValue)
                            query["QueryParams.PageSize"] = qp.PageSize.Value.ToString();

                        if (!string.IsNullOrEmpty(qp.SortBy))
                            query["QueryParams.SortBy"] = qp.SortBy;

                        if (qp.SortDescending.HasValue)
                            query["QueryParams.SortDescending"] = qp.SortDescending.Value.ToString().ToLower();
                    }
                }

                uriBuilder.Query = query.ToString();

                var response = await _httpClient.GetAsync(uriBuilder.Uri);

                var result = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResponseDto<UserResponseDto>>>();
                return result ?? new ApiResponse<PagedResponseDto<UserResponseDto>>();
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<PagedResponseDto<UserResponseDto>>
                {
                    Success = false,
                    Message = $"Network error: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<PagedResponseDto<UserResponseDto>>
                {
                    Success = false,
                    Message = $"Unexpected error: {ex.Message}"
                };
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

        public async Task<ApiResponse<UserResponseDto>> PostUserAsync(PostUserRequestDto payload)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/users", payload);
                return await response.Content.ReadFromJsonAsync<ApiResponse<UserResponseDto>>() ?? new ApiResponse<UserResponseDto>();
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<UserResponseDto>
                {
                    Message = $"Network error: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<UserResponseDto>
                {
                    Message = $"Unexpected error: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<UserResponseDto>> PatchUserAsync(Guid id, PatchUserRequestDto payload)
        {
            try
            {
                var response = await _httpClient.PatchAsJsonAsync($"api/users/{id}", payload);
                return await response.Content.ReadFromJsonAsync<ApiResponse<UserResponseDto>>() ?? new ApiResponse<UserResponseDto>();
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<UserResponseDto>
                {
                    Message = $"Network error: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<UserResponseDto>
                {
                    Message = $"Unexpected error: {ex.Message}"
                };
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