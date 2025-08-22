using FlowManager.Client.DTOs;
using FlowManager.Shared.DTOs.Requests;
using FlowManager.Shared.DTOs.Requests.Team;
using FlowManager.Shared.DTOs.Responses;
using FlowManager.Shared.DTOs.Responses.Team;
using System.Net.Http.Json;

namespace FlowManager.Client.Services
{
    public class TeamService
    {
        private readonly HttpClient _httpClient;

        public TeamService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ApiResponse<PagedResponseDto<TeamResponseDto>>> GetAllTeamsQueriedAsync(QueriedTeamRequestDto? payload = null)
        {
            try
            {
                UriBuilder uriBuilder = new UriBuilder(_httpClient.BaseAddress!)
                {
                    Path = "api/teams/queried"
                };

                var query = System.Web.HttpUtility.ParseQueryString(string.Empty);

                if (payload != null)
                {
                    if (!string.IsNullOrEmpty(payload.Name))
                    {
                        query["Name"] = payload.Name;
                    }

                    if (payload.QueryParams != null)
                    {
                        QueryParamsDto qp = payload.QueryParams;
                        if (qp.Page.HasValue)
                        {
                            query["QueryParams.Page"] = qp.Page.Value.ToString();
                        }
                        if (qp.PageSize.HasValue)
                        {
                            query["QueryParams.PageSize"] = qp.PageSize.Value.ToString();
                        }
                        if (!string.IsNullOrEmpty(qp.SortBy))
                        {
                            query["QueryParams.SortBy"] = qp.SortBy;
                        }
                        if (qp.SortDescending.HasValue)
                        {
                            query["QueryParams.SortDescending"] = qp.SortDescending.Value.ToString().ToLower();
                        }
                    }
                }

                uriBuilder.Query = query.ToString();

                HttpResponseMessage? response = await _httpClient.GetAsync(uriBuilder.Uri);

                if(response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ApiResponse<PagedResponseDto<TeamResponseDto>>>() ??
                        new ApiResponse<PagedResponseDto<TeamResponseDto>>();
                }
                else
                {
                    return new ApiResponse<PagedResponseDto<TeamResponseDto>>
                    {
                        Success = false,
                        Message = $"Error: {response.ReasonPhrase}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<PagedResponseDto<TeamResponseDto>>
                {
                    Success = false,
                    Message = $"Unexpected error: {ex.Message}"
                };
            }
        }
    }
}
