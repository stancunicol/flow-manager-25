using FlowManager.Shared.DTOs.Requests.Component;
using FlowManager.Shared.DTOs.Responses;
using FlowManager.Shared.DTOs.Responses.Component;
using System.Net.Http.Json;
using System.Text.Json;
using System.Web;

namespace FlowManager.Client.Services
{
    // Wrapper pentru răspunsurile API-ului
    public class ApiResponseWrapper<T>
    {
        public T Result { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class ComponentService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ComponentService> _logger;

        public ComponentService(HttpClient httpClient, ILogger<ComponentService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<PagedResponseDto<ComponentResponseDto>?> GetAllComponentsQueriedAsync(QueriedComponentRequestDto? payload = null)
        {
            try
            {
                var uriBuilder = new UriBuilder(_httpClient.BaseAddress!)
                {
                    Path = "api/components/queried"
                };

                var query = HttpUtility.ParseQueryString(string.Empty);

                if (payload != null)
                {
                    if (!string.IsNullOrEmpty(payload.Label))
                        query["Label"] = payload.Label;

                    if (!string.IsNullOrEmpty(payload.Type))
                        query["Type"] = payload.Type;

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
                _logger.LogInformation($"Making API call to: {uriBuilder.Uri}");

                var response = await _httpClient.GetAsync(uriBuilder.Uri);
                _logger.LogInformation($"API Response Status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    // Citește răspunsul ca string pentru debugging
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation($"API Response Content: {responseContent}");

                    // Deserializează cu wrapper-ul corect conform controller-ului
                    var apiResponse = JsonSerializer.Deserialize<ApiResponseWrapper<PagedResponseDto<ComponentResponseDto>>>(
                        responseContent,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    if (apiResponse?.Success == true && apiResponse.Result != null)
                    {
                        _logger.LogInformation($"Successfully loaded {apiResponse.Result.Data?.Count() ?? 0} components");
                        return apiResponse.Result;
                    }
                    else
                    {
                        _logger.LogWarning($"API call failed. Success: {apiResponse?.Success}, Message: {apiResponse?.Message}");
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"API call failed with status {response.StatusCode}: {errorContent}");
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAllComponentsQueriedAsync");
                return null;
            }
        }

        public async Task<ComponentResponseDto?> GetComponentByIdAsync(Guid id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/components/{id}");
                _logger.LogInformation($"GetComponentById Response Status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponseWrapper<ComponentResponseDto>>(
                        responseContent,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    if (apiResponse?.Success == true && apiResponse.Result != null)
                    {
                        return apiResponse.Result;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in GetComponentByIdAsync for id: {id}");
                return null;
            }
        }

        public async Task<ComponentResponseDto?> PostComponentAsync(PostComponentRequestDto payload)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/components", payload);
                _logger.LogInformation($"PostComponent Response Status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponseWrapper<ComponentResponseDto>>(
                        responseContent,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    if (apiResponse?.Success == true && apiResponse.Result != null)
                    {
                        return apiResponse.Result;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in PostComponentAsync");
                return null;
            }
        }

        public async Task<ComponentResponseDto?> PatchComponentAsync(Guid id, PatchComponentRequestDto payload)
        {
            try
            {
                var response = await _httpClient.PatchAsJsonAsync($"api/components/{id}", payload);
                _logger.LogInformation($"PatchComponent Response Status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponseWrapper<ComponentResponseDto>>(
                        responseContent,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    if (apiResponse?.Success == true && apiResponse.Result != null)
                    {
                        return apiResponse.Result;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in PatchComponentAsync for id: {id}");
                return null;
            }
        }

        public async Task<bool> DeleteComponentAsync(Guid id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/components/{id}");
                _logger.LogInformation($"DeleteComponent Response Status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponseWrapper<ComponentResponseDto>>(
                        responseContent,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    return apiResponse?.Success == true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in DeleteComponentAsync for id: {id}");
                return false;
            }
        }
    }
}