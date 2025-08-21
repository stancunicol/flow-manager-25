using FlowManager.Shared.DTOs.Requests.FormResponse;
using System.Net.Http.Json;
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

        public async Task<List<FormResponseResponseDto>?> GetFormResponsesByUserAsync(Guid userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/formresponses/user/{userId}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<FormResponseApiResponse<List<FormResponseResponseDto>>>(jsonContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return apiResponse?.Result;
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user form responses: {ex.Message}");
                return null;
            }
        }
    }

    public class FormResponseApiResponse<T>
    {
        public T? Result { get; set; }
        public bool Success { get; set; }
        public string? Message { get; set; }
        public DateTime Timestamp { get; set; }
    }
}