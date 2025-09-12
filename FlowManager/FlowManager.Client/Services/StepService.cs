using FlowManager.Client.DTOs;
using FlowManager.Domain.Entities;
using FlowManager.Shared.DTOs.Requests.Step;
using FlowManager.Shared.DTOs.Responses;
using FlowManager.Shared.DTOs.Responses.Step;
using FlowManager.Shared.DTOs.Responses.User;
using System.Net.Http.Json;
using System.Text.Json;
using System.Web;

namespace FlowManager.Client.Services
{
    public class StepService
    {
        private readonly HttpClient _httpClient;

        public StepService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ApiResponse<PagedResponseDto<StepResponseDto>>> GetStepsQueriedAsync(QueriedStepRequestDto? payload = null)
        {
            try
            {
                var uriBuilder = new UriBuilder(_httpClient.BaseAddress!)
                {
                    Path = "api/steps/queried"
                };

                var query = HttpUtility.ParseQueryString(string.Empty);

                if (payload != null)
                {
                    if (!string.IsNullOrEmpty(payload.Name))
                        query["Name"] = payload.Name;

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

                var result = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResponseDto<StepResponseDto>>>();
                return result ?? new ApiResponse<PagedResponseDto<StepResponseDto>>();
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<PagedResponseDto<StepResponseDto>>
                {
                    Success = false,
                    Message = $"Network error: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<PagedResponseDto<StepResponseDto>>
                {
                    Success = false,
                    Message = $"Unexpected error: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<PagedResponseDto<StepResponseDto>>> GetAllStepsIncludeUsersAndTeamsQueriedAsync(QueriedStepRequestDto? payload = null)
        {
            try
            {
                var uriBuilder = new UriBuilder(_httpClient.BaseAddress!)
                {
                    Path = "api/steps/include-teams-users/queried"
                };

                var query = HttpUtility.ParseQueryString(string.Empty);

                if (payload != null)
                {
                    if (!string.IsNullOrEmpty(payload.Name))
                        query["Name"] = payload.Name;

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

                var result = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResponseDto<StepResponseDto>>>();
                return result ?? new ApiResponse<PagedResponseDto<StepResponseDto>>();
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<PagedResponseDto<StepResponseDto>>
                {
                    Success = false,
                    Message = $"Network error: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<PagedResponseDto<StepResponseDto>>
                {
                    Success = false,
                    Message = $"Unexpected error: {ex.Message}"
                };
            }
        }

        public async Task<StepResponseDto?> GetStepAsync(Guid id)
        {
            var response = await _httpClient.GetAsync($"api/steps/{id}");
            if (!response.IsSuccessStatusCode)
                return null;

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<StepResponseDto>>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            return apiResponse?.Result;
        }

        public async Task<List<Step>> GetStepsByFlowAsync(Guid flowId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/steps/flow/{flowId}");
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

        public async Task<Step?> CreateStepAsync(Step step)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/steps", step);
                if (response.IsSuccessStatusCode)
                {
                    var wrapper = await response.Content.ReadFromJsonAsync<ApiResponse<Step>>();
                    return wrapper?.Result;
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> UpdateStepAsync(Guid id, PatchStepRequestDto payload)
        {
            try
            {
                var response = await _httpClient.PatchAsJsonAsync($"api/steps/{id}", payload);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteStepAsync(Guid id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/steps/{id}");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> AddStepToFlowAsync(Guid stepId, Guid flowId, int order = 0)
        {
            try
            {
                var response = await _httpClient.PostAsync($"api/steps/{stepId}/add-to-flow/{flowId}?order={order}", null);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemoveStepFromFlowAsync(Guid stepId, Guid flowId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/steps/{stepId}/remove-from-flow/{flowId}");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}