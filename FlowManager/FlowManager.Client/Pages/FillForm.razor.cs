using FlowManager.Client.Services;
using FlowManager.Client.DTOs;
using FlowManager.Shared.DTOs.Responses;
using FlowManager.Shared.DTOs.Responses.Component;
using FlowManager.Shared.DTOs.Responses.FormTemplate;
using FlowManager.Shared.DTOs.Responses.Step;
using FlowManager.Shared.DTOs.Responses.Flow;
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
        
        // Flow și step-uri
        private FlowResponseDto? associatedFlow;
        private StepResponseDto? firstStep;
        private bool isLoadingFlow = false;

        protected override async Task OnInitializedAsync()
        {
            await LoadCurrentUser();
            await LoadFormTemplate();
            await LoadAssociatedFlow();
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

        private async Task LoadAssociatedFlow()
        {
            if (formTemplate == null) return;

            isLoadingFlow = true;
            try
            {
                // Găsește flow-ul care are acest FormTemplateId
                var flowsResponse = await Http.GetAsync($"api/flows/queried?QueryParams.PageSize=100");
                if (flowsResponse.IsSuccessStatusCode)
                {
                    var flowsData = await flowsResponse.Content.ReadFromJsonAsync<ApiResponse<PagedResponseDto<FlowResponseDto>>>();
                    var flows = flowsData?.Result?.Data;

                    if (flows != null)
                    {
                        associatedFlow = flows.FirstOrDefault(f => f.FormTemplateId == formTemplate.Id);
                        
                        if (associatedFlow?.Steps?.Any() == true)
                        {
                            // Primul step din flow (cel cu ordinea cea mai mică sau primul din listă)
                            firstStep = associatedFlow.Steps.First();
                            Console.WriteLine($"[DEBUG] Found flow '{associatedFlow.Name}' with first step: '{firstStep.Name}'");
                        }
                        else
                        {
                            Console.WriteLine($"[WARNING] Flow found but has no steps configured");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[WARNING] No flow found for form template: {formTemplate.Name}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading associated flow: {ex.Message}");
            }
            finally
            {
                isLoadingFlow = false;
                StateHasChanged();
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

            if (firstStep == null)
            {
                await JSRuntime.InvokeVoidAsync("alert", "No workflow configured for this form template. Please contact an administrator.");
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
                    StepId = firstStep.Id, // Folosește primul step din flow
                    UserId = currentUserId,
                    ResponseFields = responses
                };

                Console.WriteLine($"[DEBUG] Submitting form: Template={TemplateId}, FirstStep={firstStep.Id} ({firstStep.Name}), User={currentUserId}");
                Console.WriteLine($"[DEBUG] Response fields count: {responses.Count}");

                var response = await Http.PostAsJsonAsync("api/formresponses", formResponseData);

                if (response.IsSuccessStatusCode)
                {
                    await JSRuntime.InvokeVoidAsync("alert", $"Form submitted successfully! It will start processing from step: {firstStep.Name}");
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

        // Metodă pentru a obține opțiunile radio button-ului din component
        private List<string> GetRadioOptions(ComponentResponseDto component)
        {
            if (component.Properties != null && component.Properties.ContainsKey("Options"))
            {
                try
                {
                    if (component.Properties["Options"] is JsonElement jsonElement)
                    {
                        var optionsList = JsonSerializer.Deserialize<List<string>>(jsonElement.GetRawText());
                        return optionsList ?? new List<string> { "Option 1", "Option 2" };
                    }
                    else if (component.Properties["Options"] is List<string> directList)
                    {
                        return directList;
                    }
                    else if (component.Properties["Options"] is string[] stringArray)
                    {
                        return stringArray.ToList();
                    }
                }
                catch
                {
                    // Fallback la opțiuni default
                }
            }

            return new List<string> { "Option 1", "Option 2" };
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