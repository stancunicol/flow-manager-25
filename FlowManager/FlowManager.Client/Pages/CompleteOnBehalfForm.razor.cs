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
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.JSInterop;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace FlowManager.Client.Pages
{
    public partial class CompleteOnBehalfForm : ComponentBase
    {
        [Inject] private NavigationManager Navigation { get; set; } = default!;
        [Inject] private FormTemplateService _formTemplateService { get; set; } = default!;
        [Inject] private FlowService _flowService { get; set; } = default!;
        [Inject] private ComponentService _componentService { get; set; } = default!;
        [Inject] private AuthService _authService { get; set; } = default!;
        [Inject] private FormResponseService _formResponseService { get; set; } = default!;
        [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

        [Parameter] public Guid TemplateId { get; set; }

        private FormTemplateVM? _formTemplate;
        private List<FormElement>? _formElements;
        private List<ComponentVM>? _components;
        private bool _isLoading = true;
        private bool _isSubmitting = false;
        private Dictionary<Guid, object> _responses = new();
        
        private List<UserVM> _availableUsers = new();
        private string _usersSearchTerm = string.Empty;
        private bool _isAvailableUsersDropdownOpen = false;
        private UserVM _selectedUserToComplete = null;

        private FlowVM? _associatedFlow;
        private StepVM? _firstStep;
        private bool _isLoadingFlow = false;

        protected override async Task OnInitializedAsync()
        {
            await LoadFormTemplate();
            await LoadAssociatedFlow();
        }

        private async Task LoadFormTemplate()
        {
            _isLoading = true;
            try
            {
                FormTemplateResponseDto? formTemplateResponse = await _formTemplateService.GetFormTemplateByIdAsync(TemplateId);

                if(formTemplateResponse == null)
                {
                    return;
                }

                _formTemplate = new FormTemplateVM
                {
                    Id = formTemplateResponse.Id,
                    Name = formTemplateResponse.Name,
                    Content = formTemplateResponse.Content,
                };

                await ParseFormContent();
                await LoadComponents();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading form template: {ex.Message}");
            }
            finally
            {
                _isLoading = false;
                StateHasChanged();
            }
        }

        private async Task LoadAssociatedFlow()
        {
            if (_formTemplate == null) 
                return;

            _isLoadingFlow = true;

            ApiResponse<FlowResponseDto?> flowResponse = await _flowService.GetFlowByFormTemplateIdAsync(_formTemplate.Id);

            if (flowResponse == null || !flowResponse.Success)
            {
                return;
            }

            _associatedFlow = new FlowVM
            {
                Id = flowResponse.Result.Id,
                Name = flowResponse.Result.Name,
                Steps = flowResponse.Result.FlowSteps.Select(fs => new FlowStepVM
                {
                    Id = fs.Id,
                    Step = new StepVM
                    {
                        Id = fs.StepId ?? Guid.Empty,
                        Name = fs.StepName,
                    }
                }).ToList()
            };

            if(_associatedFlow.Steps.Count > 0 ) 
            {
                _firstStep = _associatedFlow.Steps.First().Step;
            }
            else
            {
                _firstStep = null;
                Console.WriteLine("Flow does not contain steps!");
            }

            _isLoadingFlow = false;
            StateHasChanged();
        }

        private async Task ParseFormContent()
        {
            if (string.IsNullOrEmpty(_formTemplate?.Content))
                return;

            try
            {
                var contentData = JsonSerializer.Deserialize<FormContent>(_formTemplate.Content);
                _formElements = contentData?.Elements?.ToList() ?? new List<FormElement>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing form content: {ex.Message}");
                _formElements = new List<FormElement>();
            }
        }

        private async Task LoadComponents()
        {
            if (_formTemplate?.Components?.Any() != true)
                return;

            try
            {
                var componentTasks = _formTemplate.Components.Select(async formTemplateComponent =>
                {
                    try
                    {
                        return await _componentService.GetComponentByIdAsync(formTemplateComponent.Id);
                    }
                    catch
                    {
                        return null;
                    }
                });

                var componentResults = await Task.WhenAll(componentTasks);
                _components = componentResults.Where(c => c != null).Select(c => new ComponentVM
                {
                    Id = c.Id,
                    Type = c.Type,
                    Label = c.Label,
                    Required = c.Required,
                    Properties = c.Properties
                }).ToList()!;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading components: {ex.Message}");
                _components = new List<ComponentVM>();
            }
        }

        private void UpdateResponse(Guid componentId, string? value, string componentType)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                _responses.Remove(componentId);
                return;
            }

            object convertedValue = componentType.ToLower() switch
            {
                "number" => int.TryParse(value, out var intVal) ? intVal : value,
                "checkbox" => bool.TryParse(value, out var boolVal) ? boolVal : value,
                "datetime" => DateTime.TryParse(value, out var dateVal) ? dateVal : value,
                _ => value
            };

            _responses[componentId] = convertedValue;
        }

        private async Task SubmitForm()
        {
            var requiredComponents = _components?.Where(c => c.Required == true) ?? new List<ComponentVM>();
            var missingRequiredFields = requiredComponents.Where(c => !_responses.ContainsKey(c.Id)).ToList();

            _isSubmitting = true;
            StateHasChanged();

            try
            {
                var formResponseData = new PostFormResponseRequestDto
                {
                    FormTemplateId = TemplateId,
                    StepId = _firstStep.Id,
                    UserId = _selectedUserToComplete.Id,
                    CompletedByOtherUserId = (await _authService.GetCurrentUserAsync())!.Id,
                    ResponseFields = _responses
                };

                var response = await _formResponseService.SubmitFormResponseAsync(formResponseData);

                await JSRuntime.InvokeVoidAsync("alert", $"Form submitted successfully!");
                Navigation.NavigateTo("/basic-user");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Exception during submit: {ex.Message}");
                await JSRuntime.InvokeVoidAsync("alert", "Error submitting form. Please try again.");
            }
            finally
            {
                _isSubmitting = false;
                StateHasChanged();
            }
        }

        private void SelectUser(UserVM user)
        {
            _selectedUserToComplete = user;
            _isAvailableUsersDropdownOpen = false;
            _usersSearchTerm = user.Name;
        }

        private void OpenAvailableUsersDropdown()
        {
            _isAvailableUsersDropdownOpen = true;
        }

        private void GoBack()
        {
            Navigation.NavigateTo("/basic-user");
        }

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