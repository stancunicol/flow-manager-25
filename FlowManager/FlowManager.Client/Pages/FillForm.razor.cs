using FlowManager.Client.Services;
using FlowManager.Client.DTOs;
using FlowManager.Shared.DTOs.Responses.Component;
using FlowManager.Shared.DTOs.Responses.FormTemplate;
using FlowManager.Shared.DTOs.Responses.Step;
using FlowManager.Shared.DTOs.Requests.FormResponse;
using FlowManager.Shared.DTOs;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using System.Text.Json;

namespace FlowManager.Client.Pages
{
    public partial class FillForm : ComponentBase
    {
        [Parameter] public Guid TemplateId { get; set; }

        private FormTemplateResponseDto? formTemplate;
        private List<FormElement>? formElements;
        private List<ComponentResponseDto>? components;
        private bool isLoading = true;
        private bool isSubmitting = false;
        private Dictionary<Guid, object> responses = new();
        private Guid currentUserId = Guid.Empty;
        private Guid? selectedStepId = null;
        private List<StepResponseDto>? availableSteps;

        protected override async Task OnInitializedAsync()
        {
            await LoadCurrentUser();
            await LoadFormTemplate();
            await LoadAvailableSteps();
        }

        private async Task LoadCurrentUser()
        {
            try
            {
                var response = await Http.GetAsync("api/auth/me");
                if (response.IsSuccessStatusCode)
                {
                    var userInfo = await response.Content.ReadFromJsonAsync<UserProfileDto>();
                    if (userInfo != null && userInfo.Id != Guid.Empty)
                    {
                        currentUserId = userInfo.Id;
                        Console.WriteLine($"[DEBUG] Current user ID: {currentUserId}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to load current user: {ex.Message}");
            }
        }

        private async Task LoadFormTemplate()
        {
            isLoading = true;
            try
            {
                formTemplate = await FormTemplateService.GetFormTemplateByIdAsync(TemplateId);
                if (formTemplate != null)
                {
                    await ParseFormContent();
                    await LoadComponents();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading form template: {ex.Message}");
            }
            finally
            {
                isLoading = false;
                StateHasChanged();
            }
        }

        private async Task LoadAvailableSteps()
        {
            try
            {
                // Încarcă toate step-urile disponibile
                var response = await Http.GetAsync("api/steps/all");
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<StepResponseDto>>>();
                    availableSteps = result?.Result ?? new List<StepResponseDto>();

                    // Setează primul step ca default dacă există
                    if (availableSteps.Any())
                    {
                        selectedStepId = availableSteps.First().Id;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading steps: {ex.Message}");
                availableSteps = new List<StepResponseDto>();
            }
        }

        private async Task ParseFormContent()
        {
            if (string.IsNullOrEmpty(formTemplate?.Content))
                return;

            try
            {
                var contentData = JsonSerializer.Deserialize<FormContent>(formTemplate.Content);
                formElements = contentData?.Elements?.ToList() ?? new List<FormElement>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing form content: {ex.Message}");
                formElements = new List<FormElement>();
            }
        }

        private async Task LoadComponents()
        {
            if (formTemplate?.Components?.Any() != true)
                return;

            try
            {
                var componentTasks = formTemplate.Components.Select(async formTemplateComponent =>
                {
                    try
                    {
                        return await ComponentService.GetComponentByIdAsync(formTemplateComponent.Id);
                    }
                    catch
                    {
                        return null;
                    }
                });

                var componentResults = await Task.WhenAll(componentTasks);
                components = componentResults.Where(c => c != null).ToList()!;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading components: {ex.Message}");
                components = new List<ComponentResponseDto>();
            }
        }

        private void UpdateResponse(Guid componentId, string? value, string componentType)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                responses.Remove(componentId);
                return;
            }

            // Convertim valoarea în tipul corespunzător
            object convertedValue = componentType.ToLower() switch
            {
                "number" => int.TryParse(value, out var intVal) ? intVal : value,
                "checkbox" => bool.TryParse(value, out var boolVal) ? boolVal : value,
                "datetime" => DateTime.TryParse(value, out var dateVal) ? dateVal : value,
                _ => value
            };

            responses[componentId] = convertedValue;
        }

        private async Task SubmitForm()
        {
            if (currentUserId == Guid.Empty)
            {
                await JSRuntime.InvokeVoidAsync("alert", "User not authenticated. Please login again.");
                return;
            }

            if (!selectedStepId.HasValue)
            {
                await JSRuntime.InvokeVoidAsync("alert", "Please select a step for this form.");
                return;
            }

            // Validare: verifică câmpurile obligatorii
            var requiredComponents = components?.Where(c => c.Required == true) ?? new List<ComponentResponseDto>();
            var missingRequiredFields = requiredComponents.Where(c => !responses.ContainsKey(c.Id)).ToList();

            if (missingRequiredFields.Any())
            {
                var fieldNames = string.Join(", ", missingRequiredFields.Select(f => f.Label));
                await JSRuntime.InvokeVoidAsync("alert", $"Please fill in all required fields: {fieldNames}");
                return;
            }

            isSubmitting = true;
            StateHasChanged();

            try
            {
                var formResponseData = new PostFormResponseRequestDto
                {
                    FormTemplateId = TemplateId,
                    StepId = selectedStepId.Value,
                    UserId = currentUserId,
                    ResponseFields = responses
                };

                Console.WriteLine($"[DEBUG] Submitting form: Template={TemplateId}, Step={selectedStepId}, User={currentUserId}");
                Console.WriteLine($"[DEBUG] Response fields count: {responses.Count}");

                var response = await Http.PostAsJsonAsync("api/formresponses", formResponseData);

                if (response.IsSuccessStatusCode)
                {
                    await JSRuntime.InvokeVoidAsync("alert", "Form submitted successfully!");
                    Navigation.NavigateTo("/basic-user");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[ERROR] Submit failed: {response.StatusCode} - {errorContent}");
                    await JSRuntime.InvokeVoidAsync("alert", "Error submitting form. Please try again.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Exception during submit: {ex.Message}");
                await JSRuntime.InvokeVoidAsync("alert", "Error submitting form. Please try again.");
            }
            finally
            {
                isSubmitting = false;
                StateHasChanged();
            }
        }

        private void GoBack()
        {
            Navigation.NavigateTo("/basic-user");
        }

        // Classes pentru deserializare
        public class FormContent
        {
            public string Layout { get; set; } = "";
            public List<FormElement> Elements { get; set; } = new();
        }

        public class FormElement
        {
            public string Id { get; set; } = "";
            public int X { get; set; }
            public int Y { get; set; }
            public int ZIndex { get; set; }
            public bool IsTextElement { get; set; }
            public string? TextContent { get; set; }
            public Guid? ComponentId { get; set; }
            public string? ComponentType { get; set; }
            public string? Label { get; set; }
            public bool? Required { get; set; }
            public Dictionary<string, object>? Properties { get; set; }
        }

        
    }
}