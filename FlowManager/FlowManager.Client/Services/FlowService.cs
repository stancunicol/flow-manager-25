using BlazorBootstrap;
using FlowManager.Client.DTOs;
using FlowManager.Domain.Entities;
using FlowManager.Shared.DTOs.Requests.Flow;
using FlowManager.Shared.DTOs.Responses;
using FlowManager.Shared.DTOs.Responses.Flow;
using FlowManager.Shared.DTOs.Responses.User;
using System.Net.Http.Json;
using System.Web;

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

        public async Task<ApiResponse<FlowResponseDto>> PostFlowAsync(PostFlowRequestDto payload)
        {
            Console.WriteLine("dadadada");
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/flows/", payload);
                return await response.Content.ReadFromJsonAsync<ApiResponse<FlowResponseDto>>() ?? new ApiResponse<FlowResponseDto>();
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<FlowResponseDto>
                {
                    Message = $"Network error: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<FlowResponseDto>
                {
                    Message = $"Unexpected error: {ex.Message}"
                };
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

        public async Task<ApiResponse<PagedResponseDto<FlowResponseDto>>> GetAllFlowsQueriedAsync(QueriedFlowRequestDto payload)
        {
            try
            {
                var uriBuilder = new UriBuilder(_httpClient.BaseAddress!)
                {
                    Path = "api/flows/queried"
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

                var result = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResponseDto<FlowResponseDto>>>();
                return result ?? new ApiResponse<PagedResponseDto<FlowResponseDto>>();
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<PagedResponseDto<FlowResponseDto>>
                {
                    Message = $"Network error: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<PagedResponseDto<FlowResponseDto>>
                {
                    Message = $"Unexpected error: {ex.Message}"
                };
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