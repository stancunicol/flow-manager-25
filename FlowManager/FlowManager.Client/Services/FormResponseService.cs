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

        public async Task<PagedUserFormsResponse?> GetFormResponsesByUserPagedAsync(Guid userId, int page, int pageSize, string? searchTerm = null)
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
                var uriBuilder = new UriBuilder(_httpClient.BaseAddress!)
                {
                    Path = "api/formresponses/queried"
                };

                var query = HttpUtility.ParseQueryString(string.Empty);

                query["IncludeDeleted"] = "false";

                if (!string.IsNullOrEmpty(searchTerm))
                    query["SearchTerm"] = searchTerm;

                query["QueryParams.Page"] = page.ToString();
                query["QueryParams.PageSize"] = pageSize.ToString();
                query["QueryParams.SortBy"] = "CreatedAt";
                query["QueryParams.SortDescending"] = "true";

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
                        // CORECTEAZĂ și această linie:
                        var allForms = apiResponse.Result.Data != null ?
                            new List<FormResponseResponseDto>(apiResponse.Result.Data) :
                            new List<FormResponseResponseDto>();

                        Console.WriteLine($"[FormResponseService] Moderator received {allForms.Count} forms, Total: {apiResponse.Result.TotalCount}");

                        return new PagedModeratorFormsResponse
                        {
                            FormResponses = allForms,
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