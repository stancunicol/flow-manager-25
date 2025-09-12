using FlowManager.Shared.DTOs.Responses;
using FlowManager.Shared.DTOs.Responses.StepHistory;
using FlowManager.Shared.DTOs.Requests.StepHistory;
using System.Net.Http.Json;
using FlowManager.Client.DTOs;
using System.Web;
using System.Text.Json;

namespace FlowManager.Client.Services
{
    public class StepHistoryService
    {
        private readonly HttpClient _httpClient;

        public StepHistoryService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ApiResponse<PagedResponseDto<StepHistoryResponseDto>>> GetStepHistoriesQueriedAsync(QueriedStepHistoryRequestDto? payload)
        {
            try
            {
                var uriBuilder = new UriBuilder(_httpClient.BaseAddress!)
                {
                    Path = "api/stephistory/queried"
                };

                var query = HttpUtility.ParseQueryString(string.Empty);

                if (payload != null)
                {

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

                var result = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResponseDto<StepHistoryResponseDto>>>();
                return result ?? new ApiResponse<PagedResponseDto<StepHistoryResponseDto>>();
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<PagedResponseDto<StepHistoryResponseDto>>
                {
                    Success = false,
                    Message = $"Network error: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<PagedResponseDto<StepHistoryResponseDto>>
                {
                    Success = false,
                    Message = $"Unexpected error: {ex.Message}"
                };
            }
        }

        public async Task<List<StepHistoryResponseDto>?> GetAllAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/stephistory/all");
                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<StepHistoryResponseDto>>>();
                    return apiResponse?.Result ?? new List<StepHistoryResponseDto>();
                }
                return new List<StepHistoryResponseDto>();
            }
            catch
            {
                return new List<StepHistoryResponseDto>();
            }
        }

        public async Task<StepHistoryResponseDto?> GetByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"api/stephistory/{id}");
            if (!response.IsSuccessStatusCode)
                return null;

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<StepHistoryResponseDto>>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            return apiResponse?.Result;
        }

        public async Task<StepHistoryResponseDto?> CreateStepHistoryForNameChangeAsync(CreateStepHistoryRequestDto payload)
        {
            var response = await _httpClient.PostAsJsonAsync("api/stephistory/change-name", payload);
            if (!response.IsSuccessStatusCode)
                return null;

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<StepHistoryResponseDto>>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            return apiResponse?.Result;
        }

        public async Task<StepHistoryResponseDto?> CreateStepHistoryForMoveUsersAsync(CreateStepHistoryRequestDto payload)
        {
            var response = await _httpClient.PostAsJsonAsync("api/stephistory/move-users", payload);
            if (!response.IsSuccessStatusCode)
                return null;

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<StepHistoryResponseDto>>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            return apiResponse?.Result;
        }

        public async Task<StepHistoryResponseDto?> CreateStepHistoryForCreateDepartmentAsync(CreateStepHistoryRequestDto payload)
        {
            var response = await _httpClient.PostAsJsonAsync("api/stephistory/create-department", payload);
            if (!response.IsSuccessStatusCode)
                return null;

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<StepHistoryResponseDto>>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            return apiResponse?.Result;
        }

        public async Task<StepHistoryResponseDto?> CreateStepHistoryForDeleteDepartmentAsync(CreateStepHistoryRequestDto payload)
        {
            var response = await _httpClient.PostAsJsonAsync("api/stephistory/delete-department", payload);
            if (!response.IsSuccessStatusCode)
                return null;

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<StepHistoryResponseDto>>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            return apiResponse?.Result;
        }
    }
}