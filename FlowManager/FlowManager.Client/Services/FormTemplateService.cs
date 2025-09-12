using FlowManager.Client.DTOs;
using FlowManager.Shared.DTOs.Requests;
using FlowManager.Shared.DTOs.Requests.FormTemplate;
using FlowManager.Shared.DTOs.Responses;
using FlowManager.Shared.DTOs.Responses.FormTemplate;
using FlowManager.Shared.DTOs.Requests;
using FlowManager.Client.DTOs;
using System.Net.Http.Json;
using System.Text.Json;
using System.Web;

namespace FlowManager.Client.Services
{
    public class FormTemplateService
    {
        private readonly HttpClient _httpClient;

        public FormTemplateService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<FormTemplateResponseDto>?> GetActiveFormTemplatesAsync()
        {
            try
            {
                Console.WriteLine("[FormTemplateService] Loading active form templates...");

                var response = await _httpClient.GetAsync("api/formtemplates/queried?QueryParams.PageSize=1000");

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[FormTemplateService] Raw response: {jsonContent}");

                    var apiResponse = JsonSerializer.Deserialize<FormTemplateApiResponse<PagedResponseDto<FormTemplateResponseDto>>>(jsonContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    var allTemplates = apiResponse?.Result?.Data?.ToList() ?? new List<FormTemplateResponseDto>();
                    Console.WriteLine($"[FormTemplateService] Found {allTemplates.Count} total templates");

                    var activeTemplates = allTemplates
                        .Where(t => t.FlowId.HasValue) 
                        .GroupBy(t => t.FlowId) 
                        .Select(group => group
                            .OrderByDescending(t => t.CreatedAt) 
                            .First())
                        .ToList();

                    Console.WriteLine($"[FormTemplateService] Filtered to {activeTemplates.Count} active templates");

                    return activeTemplates;
                }
                return new List<FormTemplateResponseDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FormTemplateService] Error loading active templates: {ex.Message}");
                return new List<FormTemplateResponseDto>();
            }
        }

        public async Task<PagedTemplatesResponse?> GetActiveFormTemplatesPagedAsync(int page, int pageSize, string? searchTerm = null)
        {
            try
            {
                var activeTemplates = await GetActiveFormTemplatesAsync();

                if (activeTemplates == null)
                {
                    return new PagedTemplatesResponse
                    {
                        Templates = new List<FormTemplateResponseDto>(),
                        TotalCount = 0,
                        Page = page,
                        PageSize = pageSize,
                        HasMore = false
                    };
                }

                var filteredTemplates = activeTemplates;

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var search = searchTerm.ToLower();
                    filteredTemplates = activeTemplates
                        .Where(t => t.Name != null && t.Name.ToLower().Contains(search))
                        .ToList();
                }

                var totalCount = filteredTemplates.Count;
                var skip = (page - 1) * pageSize;
                var pagedTemplates = filteredTemplates
                    .Skip(skip)
                    .Take(pageSize)
                    .ToList();

                return new PagedTemplatesResponse
                {
                    Templates = pagedTemplates,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    HasMore = skip + pageSize < totalCount
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FormTemplateService] Error in GetActiveFormTemplatesPagedAsync: {ex.Message}");
                return new PagedTemplatesResponse
                {
                    Templates = new List<FormTemplateResponseDto>(),
                    TotalCount = 0,
                    Page = page,
                    PageSize = pageSize,
                    HasMore = false
                };
            }
        }

        public async Task<List<FormTemplateResponseDto>?> GetAllFormTemplatesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/formtemplates/queried");

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Raw response: {jsonContent}");

                    var apiResponse = JsonSerializer.Deserialize<FormTemplateApiResponse<PagedResponseDto<FormTemplateResponseDto>>>(jsonContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return apiResponse?.Result?.Data?.ToList();
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading templates: {ex.Message}");
                return null;
            }
        }

        public async Task<PagedTemplatesResponse?> GetFormTemplatesPagedAsync(int page, int pageSize, string? searchTerm = null)
        {
            return await GetActiveFormTemplatesPagedAsync(page, pageSize, searchTerm);
        }

        public async Task<PagedResponseDto<FormTemplateResponseDto>?> GetAllFormTemplatesQueriedAsync(QueriedFormTemplateRequestDto? payload = null)
        {
            try
            {
                var uriBuilder = new UriBuilder(_httpClient.BaseAddress!)
                {
                    Path = "api/formtemplates/queried"
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

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<FormTemplateApiResponse<PagedResponseDto<FormTemplateResponseDto>>>(jsonContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return apiResponse?.Result;
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting queried form templates: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> GetFormTemplateNameUnicityAsync(string formTemplateName)
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"api/formTemplates/form-template-valid/{formTemplateName}");

            ApiResponse<bool>? result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();

            if (result == null)
                return false;

            return result.Result;
        }

        public async Task<FormTemplateResponseDto?> GetFormTemplateByIdAsync(Guid id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/formtemplates/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<FormTemplateApiResponse<FormTemplateResponseDto>>(jsonContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return apiResponse?.Result;
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting form template by id: {ex.Message}");
                return null;
            }
        }

        public async Task<FormTemplateResponseDto?> PostFormTemplateAsync(PostFormTemplateRequestDto payload)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/formtemplates", payload);

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<FormTemplateApiResponse<FormTemplateResponseDto>>(jsonContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return apiResponse?.Result;
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error posting form template: {ex.Message}");
                return null;
            }
        }

        public async Task<FormTemplateResponseDto?> PatchFormTemplateAsync(Guid id, PatchFormTemplateRequestDto payload)
        {
            try
            {
                var response = await _httpClient.PatchAsJsonAsync($"api/formtemplates/{id}", payload);

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<FormTemplateApiResponse<FormTemplateResponseDto>>(jsonContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return apiResponse?.Result;
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error patching form template: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> DeleteFormTemplateAsync(Guid id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/formtemplates/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting form template: {ex.Message}");
                return false;
            }
        }
    }
}