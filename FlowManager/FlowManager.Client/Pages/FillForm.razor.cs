using FlowManager.Client.Deserialization;
using FlowManager.Client.DTOs;
using FlowManager.Client.Services;
using FlowManager.Client.ViewModels;
using FlowManager.Shared.DTOs;
using FlowManager.Shared.DTOs.Requests.FormResponse;
using FlowManager.Shared.DTOs.Responses;
using FlowManager.Shared.DTOs.Responses.Component;
using FlowManager.Shared.DTOs.Responses.Flow;
using FlowManager.Shared.DTOs.Responses.FormTemplate;
using FlowManager.Shared.DTOs.Responses.Step;
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
        private List<ComponentVM>? componentVMs;
        private bool isLoading = true;
        private bool isSubmitting = false;
        private Dictionary<Guid, object> responses = new();
        private Guid currentUserId = Guid.Empty;
        private UserVM? currentUser;

        // Flow and steps
        private FlowResponseDto? associatedFlow;
        private StepResponseDto? firstStep;
        private bool isLoadingFlow = false;

        // Auto-fill functionality
        private bool _showComponentSelector = false;
        private bool _showUserSelector = false;
        private UserVM? _selectedUserForAutoFill;
        private Dictionary<Guid, bool> _autoFilledFields = new();

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
                        currentUser = new UserVM
                        {
                            Id = userInfo.Id,
                            Name = userInfo.Name,
                            Email = userInfo.Email,
                            PhoneNumber = userInfo.PhoneNumber
                        };
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

                componentVMs = new List<ComponentVM>();
                if (formElements != null)
                {
                    foreach (var element in formElements.Where(e => !e.IsTextElement && e.ComponentId.HasValue))
                    {
                        var component = components.FirstOrDefault(c => c.Id == element.ComponentId);
                        if (component != null)
                        {
                            componentVMs.Add(new ComponentVM
                            {
                                Id = Guid.Parse(element.Id), 
                                Type = component.Type,
                                Label = component.Label,
                                Required = component.Required,
                                Properties = component.Properties
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading components: {ex.Message}");
                components = new List<ComponentResponseDto>();
                componentVMs = new List<ComponentVM>();
            }
        }

        // Auto-fill methods
        private void StartAutoFillForCurrentUser()
        {
            Console.WriteLine("[DEBUG] StartAutoFillForCurrentUser called");
            Console.WriteLine($"[DEBUG] Current user: {currentUser?.Name ?? "NULL"}");
            Console.WriteLine($"[DEBUG] ComponentVMs count: {componentVMs?.Count ?? 0}");

            _selectedUserForAutoFill = currentUser;
            _showUserSelector = false; 
            _showComponentSelector = true; 

            Console.WriteLine($"[DEBUG] ShowComponentSelector: {_showComponentSelector}, SelectedUser: {_selectedUserForAutoFill?.Name}");
            StateHasChanged();
        }

        private void StartAutoFillForOtherUser()
        {
            Console.WriteLine("[DEBUG] StartAutoFillForOtherUser called");
            Console.WriteLine($"[DEBUG] ComponentVMs count: {componentVMs?.Count ?? 0}");

            _selectedUserForAutoFill = null;
            _showUserSelector = true; 
            _showComponentSelector = false; 

            Console.WriteLine($"[DEBUG] ShowUserSelector: {_showUserSelector}, ShowComponentSelector: {_showComponentSelector}");
            StateHasChanged();
        }

        private async Task OnUserSelectedForAutoFill(UserVM user)
        {
            Console.WriteLine($"[DEBUG] OnUserSelectedForAutoFill called with: {user?.Name ?? "NULL"}");

            _selectedUserForAutoFill = user;
            _showUserSelector = false; 
            _showComponentSelector = true; 

            Console.WriteLine($"[DEBUG] User selected, now showing component selector");
            StateHasChanged();
        }

        private async Task OnComponentsSelectedForAutoFill(List<Guid> componentIds)
        {
            Console.WriteLine($"[DEBUG] OnComponentsSelectedForAutoFill called with {componentIds.Count} components");

            if (_selectedUserForAutoFill == null)
            {
                Console.WriteLine("[DEBUG] No user selected for auto-fill");
                return;
            }

            foreach (var componentId in componentIds)
            {
                var formElement = formElements?.FirstOrDefault(e => e.Id == componentId.ToString());
                if (formElement != null && formElement.ComponentId.HasValue)
                {
                    var component = components?.FirstOrDefault(c => c.Id == formElement.ComponentId.Value);
                    if (component != null)
                    {
                        var fieldValue = GetUserDataMapping(component, _selectedUserForAutoFill);
                        if (fieldValue != null)
                        {
                            responses[componentId] = fieldValue; 
                            _autoFilledFields[componentId] = true; 
                            Console.WriteLine($"[DEBUG] Auto-filled component {component.Label} with value: {fieldValue}");
                        }
                    }
                }
            }

            _showComponentSelector = false;
            StateHasChanged();
        }

        private void CloseComponentSelector()
        {
            Console.WriteLine("[DEBUG] CloseComponentSelector called");
            _showComponentSelector = false;
            StateHasChanged();
        }

        private void CloseUserSelector()
        {
            Console.WriteLine("[DEBUG] CloseUserSelector called");
            _showUserSelector = false;
            _selectedUserForAutoFill = null;
            StateHasChanged();
        }

        private object? GetUserDataMapping(ComponentResponseDto component, UserVM user)
        {
            string label = component.Label?.ToLower() ?? "";

            return label switch
            {
                var l when l.Contains("email") || l.Contains("e-mail") || l.Contains("mail") => user.Email,
                var l when l.Contains("phone") || l.Contains("telefon") || l.Contains("mobil") => user.PhoneNumber,
                var l when l.Contains("name") || l.Contains("nume") || l.Contains("prenume") => user.Name,
                var l when l.Contains("department") || l.Contains("step") => user.Step?.Name ?? null,
                _ => null
            };
        }

        private bool IsFieldAutoFilled(Guid componentId)
        {
            return _autoFilledFields.ContainsKey(componentId) && _autoFilledFields[componentId];
        }

        private string GetFieldValue(Guid componentId)
        {
            if (responses.ContainsKey(componentId))
            {
                return responses[componentId]?.ToString() ?? string.Empty;
            }
            return string.Empty;
        }

        private void UpdateResponse(Guid componentId, string? value, string componentType)
        {
            if (_autoFilledFields.ContainsKey(componentId))
            {
                _autoFilledFields[componentId] = false;
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                responses.Remove(componentId);
                return;
            }

            object convertedValue = componentType.ToLower() switch
            {
                "number" => int.TryParse(value, out var intVal) ? intVal : value,
                "checkbox" => bool.TryParse(value, out var boolVal) ? boolVal : value,
                "datetime" => DateTime.TryParse(value, out var dateVal) ? dateVal : value,
                _ => value
            };

            responses[componentId] = convertedValue;
        }

        private bool IsSubmitValid()
        {
            if (isSubmitting || currentUserId == Guid.Empty || firstStep == null || formElements == null)
                return false;

            var requiredElementIds = formElements
                .Where(e => !e.IsTextElement && e.ComponentId.HasValue)
                .Where(e => components?.FirstOrDefault(c => c.Id == e.ComponentId)?.Required == true)
                .Select(e => Guid.Parse(e.Id))
                .ToList();

            var missingRequiredFields = requiredElementIds.Where(id => !responses.ContainsKey(id)).ToList();

            return !missingRequiredFields.Any();
        }

        private async Task SubmitForm()
        {
            isSubmitting = true;
            StateHasChanged();

            try
            {
                var componentResponses = new Dictionary<Guid, object>();

                foreach (var responseEntry in responses)
                {
                    var formElementId = responseEntry.Key.ToString();
                    var formElement = formElements?.FirstOrDefault(e => e.Id == formElementId);

                    if (formElement?.ComponentId.HasValue == true)
                    {
                        componentResponses[formElement.ComponentId.Value] = responseEntry.Value;
                    }
                }

                var formResponseData = new PostFormResponseRequestDto
                {
                    FormTemplateId = TemplateId,
                    StepId = firstStep?.Id ?? Guid.Empty,
                    UserId = currentUserId,
                    ResponseFields = componentResponses 
                };

                Console.WriteLine($"[DEBUG] Submitting form: Template={TemplateId}, FirstStep={firstStep?.Id} ({firstStep?.Name}), User={currentUserId}");
                Console.WriteLine($"[DEBUG] Response fields count: {responses.Count}");

                var response = await Http.PostAsJsonAsync("api/formresponses", formResponseData);

                if (response.IsSuccessStatusCode)
                {
                    await JSRuntime.InvokeVoidAsync("alert", $"Form submitted successfully! It will start processing from step: {firstStep?.Name ?? "Unknown"}");
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
    }
}