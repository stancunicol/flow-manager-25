using FlowManager.Shared.DTOs.Requests.FormResponse;
using FlowManager.Client.DTOs;
using FlowManager.Shared.DTOs.Requests;
using FlowManager.Shared.DTOs.Responses;
using System.Net.Http.Json;
using System.Web;
using System.Text.Json;

namespace FlowManager.Client.Services
{
    public class FormResponseService
    {
        private readonly HttpClient _httpClient;

        public FormResponseService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<PagedResponseDto<FormResponseResponseDto>> GetAllFormResponsesQueriedAsync(QueriedFormResponseRequestDto payload)
        {
            try
            {
                Console.WriteLine($"[FormResponseService] Making API call to get all form responses queried with payload: {System.Text.Json.JsonSerializer.Serialize(payload)}");

                var response = await _httpClient.PostAsJsonAsync("api/FormResponses/queried", payload);

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResponseDto<FormResponseResponseDto>>>();

                    if (apiResponse?.Result != null)
                    {
                        Console.WriteLine($"[FormResponseService] Successfully retrieved {apiResponse.Result.Data?.Count() ?? 0} form responses (total: {apiResponse.Result.TotalCount})");
                        return apiResponse.Result;
                    }
                    else
                    {
                        Console.WriteLine("[FormResponseService] API returned null result");
                        return new PagedResponseDto<FormResponseResponseDto>
                        {
                            Data = new List<FormResponseResponseDto>(),
                            TotalCount = 0,
                            Page = payload.QueryParams?.Page ?? 1,
                            PageSize = payload.QueryParams?.PageSize ?? 10
                        };
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[FormResponseService] API call failed with status {response.StatusCode}: {errorContent}");

                    return new PagedResponseDto<FormResponseResponseDto>
                    {
                        Data = new List<FormResponseResponseDto>(),
                        TotalCount = 0,
                        Page = payload.QueryParams?.Page ?? 1,
                        PageSize = payload.QueryParams?.PageSize ?? 10
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FormResponseService] Exception during API call: {ex.Message}");
                throw;
            }
        }

        public async Task<List<FormResponseResponseDto>> GetFormResponsesByUserAsync(Guid userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/formresponses/user/{userId}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<FormResponseResponseDto>>>();
                    return result?.Result ?? new List<FormResponseResponseDto>();
                }

                return new List<FormResponseResponseDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading user forms: {ex.Message}");
                return new List<FormResponseResponseDto>();
            }
        }

        public async Task<PagedUserFormsResponse?> GetFormResponsesByUserPagedAsync(Guid userId, int page, int pageSize, string? searchTerm = null, List<string>? statusFilters = null)
        {
            try
            {
                var uriBuilder = new UriBuilder(_httpClient.BaseAddress!)
                {
                    Path = "api/formresponses/queried"
                };

                var query = HttpUtility.ParseQueryString(string.Empty);

                query["UserId"] = userId.ToString();
                query["IncludeDeleted"] = "false";

                if (!string.IsNullOrEmpty(searchTerm))
                    query["SearchTerm"] = searchTerm;

                // TEMPORAR: Trimite întotdeauna statusFilters pentru debugging
                if (statusFilters?.Any() == true)
                {
                    for (int i = 0; i < statusFilters.Count; i++)
                    {
                        query[$"StatusFilters[{i}]"] = statusFilters[i];
                    }
                }

                query["QueryParams.Page"] = page.ToString();
                query["QueryParams.PageSize"] = pageSize.ToString();
                query["QueryParams.SortBy"] = "CreatedAt";
                query["QueryParams.SortDescending"] = "true";

                uriBuilder.Query = query.ToString();

                Console.WriteLine($"[FormResponseService] Request URL: {uriBuilder.Uri}");

                var response = await _httpClient.GetAsync(uriBuilder.Uri);

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<FormResponseApiResponse>(jsonContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (apiResponse?.Result != null)
                    {
                        var formsList = apiResponse.Result.Data != null ?
                            new List<FormResponseResponseDto>(apiResponse.Result.Data) :
                            new List<FormResponseResponseDto>();

                        return new PagedUserFormsResponse
                        {
                            FormResponses = formsList,
                            TotalCount = apiResponse.Result.TotalCount,
                            Page = apiResponse.Result.Page,
                            PageSize = apiResponse.Result.PageSize,
                            HasMore = apiResponse.Result.HasNextPage
                        };
                    }
                }
                else
                {
                    Console.WriteLine($"[FormResponseService] Request failed: {response.StatusCode}");
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting paged user forms: {ex.Message}");
                return null;
            }
        }

        public async Task<PagedModeratorFormsResponse?> GetFormResponsesAssignedToModeratorAsync(Guid moderatorId, int page, int pageSize, string? searchTerm = null)
        {
            try
            {
                // SCHIMBAT: Folosește endpoint-ul specific pentru moderator
                var uriBuilder = new UriBuilder(_httpClient.BaseAddress!)
                {
                    Path = $"api/formresponses/assigned-to-moderator/{moderatorId}"
                };

                var query = HttpUtility.ParseQueryString(string.Empty);

                // Parametrii pentru paginare
                query["QueryParams.Page"] = page.ToString();
                query["QueryParams.PageSize"] = pageSize.ToString();
                query["QueryParams.SortBy"] = "CreatedAt";
                query["QueryParams.SortDescending"] = "true";

                query["IncludeDeleted"] = "false";

                if (!string.IsNullOrEmpty(searchTerm))
                    query["SearchTerm"] = searchTerm;

                uriBuilder.Query = query.ToString();

                Console.WriteLine($"[FormResponseService] Moderator Request URL: {uriBuilder.Uri}");

                var response = await _httpClient.GetAsync(uriBuilder.Uri);

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<FormResponseApiResponse>(jsonContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (apiResponse?.Result != null)
                    {
                        var moderatorForms = apiResponse.Result.Data != null ?
                            new List<FormResponseResponseDto>(apiResponse.Result.Data) :
                            new List<FormResponseResponseDto>();

                        Console.WriteLine($"[FormResponseService] Moderator {moderatorId} received {moderatorForms.Count} assigned forms, Total: {apiResponse.Result.TotalCount}");

                        return new PagedModeratorFormsResponse
                        {
                            FormResponses = moderatorForms,
                            TotalCount = apiResponse.Result.TotalCount,
                            Page = apiResponse.Result.Page,
                            PageSize = apiResponse.Result.PageSize,
                            HasMore = apiResponse.Result.HasNextPage
                        };
                    }
                }
                else
                {
                    Console.WriteLine($"[FormResponseService] Moderator request failed: {response.StatusCode}");
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[FormResponseService] Error details: {errorContent}");
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting assigned forms to moderator: {ex.Message}");
                return null;
            }
        }

        public async Task<FormResponseResponseDto?> SubmitFormResponseAsync(PostFormResponseRequestDto formResponse)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/formresponses", formResponse);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<FormResponseResponseDto>>();
                    return result?.Result;
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error submitting form: {ex.Message}");
                return null;
            }
        }
    }

    public class FormResponseApiResponse
    {
        public PagedResponseDto<FormResponseResponseDto>? Result { get; set; }
        public bool Success { get; set; }
        public string? Message { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class PagedUserFormsResponse
    {
        public List<FormResponseResponseDto> FormResponses { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public bool HasMore { get; set; }
    }

    public class PagedModeratorFormsResponse
    {
        public List<FormResponseResponseDto> FormResponses { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public bool HasMore { get; set; }
    }
}