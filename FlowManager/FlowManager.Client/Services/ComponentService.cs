using FlowManager.Shared.DTOs.Requests.Component;
using FlowManager.Shared.DTOs.Responses;
using FlowManager.Shared.DTOs.Responses.Component;
using System.Net.Http.Json;
using System.Web;

namespace FlowManager.Client.Services
{
    public class ComponentService
    {
        private readonly HttpClient _httpClient;

        public ComponentService(HttpClient httpClient)
        {
            _httpClient = httpClient;
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

                var response = await _httpClient.GetAsync(uriBuilder.Uri);

                if (response.IsSuccessStatusCode)
                {
                    // Răspunsul tău are formatul: { Result: { Data: [...], TotalCount: ... }, Success: true, ... }
                    var apiResponse = await response.Content.ReadFromJsonAsync<dynamic>();
                    var result = await response.Content.ReadFromJsonAsync<PagedResponseDto<ComponentResponseDto>>();
                    return result;
                }

                return null;
            }
            catch (Exception ex)
            {
                // Log error if needed
                return null;
            }
        }

        public async Task<ComponentResponseDto?> GetComponentByIdAsync(Guid id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/components/{id}");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ComponentResponseDto>();
                }

                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<ComponentResponseDto?> PostComponentAsync(PostComponentRequestDto payload)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/components", payload);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ComponentResponseDto>();
                }

                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<ComponentResponseDto?> PatchComponentAsync(Guid id, PatchComponentRequestDto payload)
        {
            try
            {
                var response = await _httpClient.PatchAsJsonAsync($"api/components/{id}", payload);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ComponentResponseDto>();
                }

                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<bool> DeleteComponentAsync(Guid id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/components/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}