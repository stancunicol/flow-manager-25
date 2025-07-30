using FlowManager.Domain.Entities;
using System.Net.Http.Json;

namespace FlowManager.Client.Services
{
    public class FormService
    {
        private readonly HttpClient _httpClient;

        public FormService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Form>> GetAllFormsAsync()
        {
            try
            {
                Console.WriteLine("[DEBUG] FormService: Making API call to api/forms");
                var response = await _httpClient.GetAsync("api/forms");
                Console.WriteLine($"[DEBUG] FormService: Response status: {response.StatusCode}");
                response.EnsureSuccessStatusCode();
                var forms = await response.Content.ReadFromJsonAsync<List<Form>>() ?? new List<Form>();
                Console.WriteLine($"[DEBUG] FormService: Received {forms.Count} forms from API");
                
                // Debug: Show details of each form
                foreach (var form in forms)
                {
                    Console.WriteLine($"[DEBUG] Form {form.Id} - FlowId: {form.FlowId}, Status: {form.Status}, UserId: {form.UserId}");
                }
                
                return forms;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] FormService: Error loading forms: {ex.Message}");
                return new List<Form>();
            }
        }

        public async Task<Form?> GetFormAsync(Guid id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/forms/{id}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<Form>();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<Form>> GetFormsByUserAsync(Guid userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/forms/user/{userId}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<Form>>() ?? new List<Form>();
                }
                return new List<Form>();
            }
            catch
            {
                return new List<Form>();
            }
        }

        public async Task<Form?> CreateFormAsync(Form form)
        {
            try
            {
                // Convert Form to CreateFormDto for API call
                var createFormDto = new CreateFormDto
                {
                    FlowId = form.FlowId,
                    UserId = form.UserId,
                    Comment = form.Comment
                };

                var response = await _httpClient.PostAsJsonAsync("api/forms", createFormDto);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<Form>();
                }
                
                // Log the error response for debugging
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[ERROR] CreateFormAsync failed: {response.StatusCode} - {errorContent}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] CreateFormAsync exception: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateFormAsync(Guid id, Form form)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/forms/{id}", form);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteFormAsync(Guid id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/forms/{id}");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}