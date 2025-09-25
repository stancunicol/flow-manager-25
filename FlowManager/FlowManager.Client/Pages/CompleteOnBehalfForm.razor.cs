using FlowManager.Client.Deserialization;
using FlowManager.Client.DTOs;
using FlowManager.Client.Services;
using FlowManager.Client.ViewModels;
using FlowManager.Shared.DTOs.Requests.FormResponse;
using FlowManager.Shared.DTOs.Requests.User;
using FlowManager.Shared.DTOs.Responses;
using FlowManager.Shared.DTOs.Responses.Flow;
using FlowManager.Shared.DTOs.Responses.FormTemplate;
using FlowManager.Shared.DTOs.Responses.User;
using Microsoft.AspNetCore.Components;
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
        [Inject] private UserService _userService { get; set; } = default!;
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
        private const int _initialPageSize = 5; 
        private int _pageSize = 5;
        private int _pageNumber = 1;
        private int _totalPages = 1;
        private bool _usersHasNextPage = true;
        private int _debounceDelayMs = 250;
        private System.Threading.Timer? _debounceTimer;

        private FlowVM? _associatedFlow;
        private FlowStepVM? _firstStep;
        private bool _isLoadingFlow = false;

        protected override async Task OnInitializedAsync()
        {
            await LoadFormTemplateAsync();
            await LoadAssociatedFlowAsync();

            _pageSize = _initialPageSize;
            await LoadUsersAsync();
        }

        private async Task LoadFormTemplateAsync()
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
                    Components = formTemplateResponse.Components.Select(c => new FormTemplateComponentVM
                    {
                        Id = c.Id,
                        Label = c.Label,
                        Type = c.Type,
                        Required = c.Required,
                        Properties = c.Properties
                    }).ToList()
                };

                await ParseFormContent();
                await LoadComponentsAsync();
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

        private void OnUsersSearchTermChanged(string newSearchTerm)
        {
            _usersSearchTerm = newSearchTerm;

            _debounceTimer?.Dispose();

            _debounceTimer = new System.Threading.Timer(async _ =>
            {
                await InvokeAsync(async () =>
                {
                    _pageSize = _initialPageSize;
                    await LoadUsersAsync();
                    StateHasChanged();
                });

                _debounceTimer?.Dispose();
                _debounceTimer = null;

            }, null, _debounceDelayMs, Timeout.Infinite);
        }

        private async Task LoadAssociatedFlowAsync()
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
                FlowSteps = flowResponse.Result.FlowSteps.Select(fs => new FlowStepVM
                {
                    Id = fs.Id,
                }).ToList()
            };

            if(_associatedFlow.FlowSteps.Count > 0) 
            {
                _firstStep = new FlowStepVM();
                _firstStep.FlowStepItems = new List<FlowStepItemVM>(_associatedFlow.FlowSteps.First().FlowStepItems);
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
                Console.WriteLine($"Parsing form content : {_formTemplate.Content}");
                var contentData = JsonSerializer.Deserialize<FormContent>(_formTemplate.Content);
                _formElements = contentData?.Elements?.ToList() ?? new List<FormElement>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing form content: {ex.Message}");
                _formElements = new List<FormElement>();
            }
        }

        private async Task LoadUsersAsync()
        {
            try
            {
                QueriedUserRequestDto payload = new QueriedUserRequestDto
                {
                    GlobalSearchTerm = _usersSearchTerm,
                    QueryParams = new Shared.DTOs.Requests.QueryParamsDto
                    {
                        Page = _pageNumber,
                        PageSize = _pageSize
                    }
                };

                ApiResponse<PagedResponseDto<UserResponseDto>> response = await _userService.GetAllUsersQueriedAsync(payload);

                if (response.Success && response.Result.Data.Any())
                {
                    _availableUsers = response.Result.Data.Select(u => new UserVM
                    {
                        Id = u.Id,
                        Name = u.Name,
                        Email = u.Email,
                        PhoneNumber = u.PhoneNumber,
                        Step = u.Step != null ? new StepVM
                        {
                            Id = u.Step.StepId,
                            Name = u.Step.StepName
                        } : null,
                    }).ToList();

                    _totalPages = response.Result.TotalPages;
                    _usersHasNextPage = response.Result.HasNextPage;
                }
                else
                {
                    _availableUsers = new List<UserVM>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading users: {ex.Message}");
                _availableUsers = new List<UserVM>();
            }

            StateHasChanged();
        }

        private async Task LoadMoreUsersAsync()
        {
            _pageSize += _initialPageSize;
            await LoadUsersAsync();
        }

        private async Task LoadComponentsAsync()
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
                    StepsIds = _firstStep!.FlowStepItems.Select(flowStepItem => flowStepItem?.StepId ?? Guid.Empty).ToList(),
                    UserId = _selectedUserToComplete.Id,
                    CompletedByOtherUserId = (await _authService.GetCurrentUserAsync())!.Id,
                    ResponseFields = _responses
                };

                var response = await _formResponseService.SubmitFormResponseAsync(formResponseData);

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

        private void ChangeAvailableUsersDropdownVisibility()
        {
            _isAvailableUsersDropdownOpen = !_isAvailableUsersDropdownOpen;
        }

        private Dictionary<Guid, bool> _readonlyFields = new();

        private void SelectUser(UserVM user)
        {
            _selectedUserToComplete = user;
            _isAvailableUsersDropdownOpen = false;
            _usersSearchTerm = user.Name;

            AutoFillUserData(user);

            StateHasChanged();
        }

        private void AutoFillUserData(UserVM user)
        {
            if (_components == null || user == null) return;

            foreach (var component in _components)
            {
                var fieldMapping = GetUserDataMapping(component, user);
                if (fieldMapping != null)
                {
                    _responses[component.Id] = fieldMapping;
                    _readonlyFields[component.Id] = true;
                }
                else
                {
                    _readonlyFields[component.Id] = false;
                }
            }
        }

        private object? GetUserDataMapping(ComponentVM component, UserVM user)
        {
            string label = component.Label?.ToLower() ?? "";

            Console.Write(label);

            return label switch
            {
                var l when l.Contains("email") || l.Contains("e-mail") || l.Contains("mail") => user.Email,
                var l when l.Contains("phone") || l.Contains("telefon") || l.Contains("mobil") => user.PhoneNumber,
                var l when l.Contains("name") || l.Contains("nume") || l.Contains("prenume") => user.Name,
                var l when l.Contains("department") || l.Contains("step") => user.Step?.Name ?? null,
                var l when l.Contains("phone") || l.Contains("Phone Number") => user.PhoneNumber,
                _ => null
            };
        }

        private bool IsFieldReadonly(Guid componentId)
        {
            return _readonlyFields.ContainsKey(componentId) && _readonlyFields[componentId];
        }

        private string GetFieldValue(Guid componentId)
        {
            if (_responses.ContainsKey(componentId))
            {
                return _responses[componentId]?.ToString() ?? string.Empty;
            }
            return string.Empty;
        }

        private void UpdateResponse(Guid componentId, string? value, string componentType)
        {
            if (IsFieldReadonly(componentId))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                _responses.Remove(componentId);
                return;
            }

            object convertedValue = componentType.ToLower() switch
            {
                "number" => value.ToString(),
                "checkbox" => bool.TryParse(value, out var boolVal) ? boolVal : value,
                "datetime" => DateTime.TryParse(value, out var dateVal) ? dateVal : value,
                _ => value
            };

            _responses[componentId] = convertedValue;
        }

        private object TryParseAsNumber(string value)
        {
            // First try to parse as int
            if (int.TryParse(value, out var intVal))
                return intVal;

            // If it fails, try as long for larger numbers
            if (long.TryParse(value, out var longVal))
                return longVal;

            // If it still fails, try as double for decimal numbers
            if (double.TryParse(value, out var doubleVal))
                return doubleVal;

            // If all parsing fails, keep it as string
            return value;
        }

        private bool AllRequiredFieldsFilled()
        {
            return _components?.Where(c => c.Required == true).All(c => _responses.ContainsKey(c.Id)) ?? false;
        }

        private List<string> GetRadioOptions(ComponentVM component)
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

        private void GoBack()
        {
            Navigation.NavigateTo("/basic-user");
        }
    }
}