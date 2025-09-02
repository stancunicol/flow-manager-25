using FlowManager.Shared.DTOs.Requests.FlowStep;
using FlowManager.Shared.DTOs.Responses;
using FlowManager.Shared.DTOs.Responses.Flow;
using FlowManager.Shared.DTOs.Responses.FlowStep;
using FlowManager.Shared.DTOs;
using System.Net.Http.Json;
using System.Text.Json;
using System.Web;
using FlowManager.Client.DTOs;
using FlowManager.Shared.DTOs.Requests.Step;
using FlowManager.Shared.DTOs.Responses.Step;
using FlowManager.Domain.Entities;

namespace FlowManager.Client.Services
{
    public class FlowStepService
    {
        private readonly HttpClient _httpClient;
        public FlowStepService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<ApiResponse<PagedResponseDto<FlowStepResponseDto>>> GetFlowStepsQueriedAsync(QueriedStepRequestDto? payload = null)
        {
            try
            {
                var uriBuilder = new UriBuilder(_httpClient.BaseAddress!)
                {
                    Path = "api/flowstep/queried"
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

                var result = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResponseDto<FlowStepResponseDto>>>();
                return result ?? new ApiResponse<PagedResponseDto<FlowStepResponseDto>>();
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<PagedResponseDto<FlowStepResponseDto>>
                {
                    Success = false,
                    Message = $"Network error: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<PagedResponseDto<FlowStepResponseDto>>
                {
                    Success = false,
                    Message = $"Unexpected error: {ex.Message}"
                };
            }
        }

        public async Task<FlowStepResponseDto?> GetAllFlowStepsAsync()
        {
            var response = await _httpClient.GetAsync($"api/flowsteps");
            if (!response.IsSuccessStatusCode)
                return null;

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<FlowStepResponseDto>>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            return apiResponse?.Result;
        }

        public async Task<FlowStepResponseDto?> GetFlowStepAsync(Guid id)
        {
            var response = await _httpClient.GetAsync($"api/flowstep/{id}");
            if (!response.IsSuccessStatusCode)
                return null;

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<FlowStepResponseDto>>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            return apiResponse?.Result;
        }

        public async Task<FlowStep?> CreateFlowStepAsync(PostFlowStepRequestDto flowStep)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/flowstep", flowStep);
                if (response.IsSuccessStatusCode)
                {
                    var wrapper = await response.Content.ReadFromJsonAsync<ApiResponse<FlowStep>>();
                    return wrapper?.Result;
                }
                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}
