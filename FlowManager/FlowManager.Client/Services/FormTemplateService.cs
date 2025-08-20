using FlowManager.Shared.DTOs.Requests.FormTemplate;
using FlowManager.Shared.DTOs.Responses;
using FlowManager.Shared.DTOs.Responses.FormTemplate;
using System.Net.Http.Json;
using System.Web;
using System.Text.Json;

namespace FlowManager.Client.Services
{
    public class FormTemplateService
    {
        private readonly HttpClient _httpClient;

        public FormTemplateService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<FormTemplateResponseDto>?> GetAllFormTemplatesAsync()
        {
            try
            {
                // Folosesc endpoint-ul queried fără parametri pentru a obține toate template-urile
                var response = await _httpClient.GetAsync("api/formtemplates/queried");

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Raw response: {jsonContent}"); // Pentru debugging

                    // Dezeria structura cu Result wrappat
                    var apiResponse = JsonSerializer.Deserialize<FormTemplateApiResponse<PagedResponseDto<FormTemplateResponseDto>>>(jsonContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    // Returnez lista din PagedResponseDto
                    return apiResponse?.Result?.Data?.ToList();
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting form templates: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return null;
            }
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

    // Clasa redenumită pentru a evita conflictele
    public class FormTemplateApiResponse<T>
    {
        public T? Result { get; set; }
        public bool Success { get; set; }
        public string? Message { get; set; }
        public DateTime Timestamp { get; set; }
    }
}