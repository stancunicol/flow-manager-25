using FlowManager.Shared.DTOs.Requests.FormResponse;
using FlowManager.Client.DTOs;
using System.Net.Http.Json;

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
}