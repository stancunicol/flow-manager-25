using FlowManager.Client.Deserialization;
using FlowManager.Client.DTOs;
using FlowManager.Client.Services;
using FlowManager.Client.ViewModels;
using FlowManager.Shared.DTOs;
using FlowManager.Shared.DTOs.Requests.FormResponse;
using FlowManager.Shared.DTOs.Responses.Component;
using FlowManager.Shared.DTOs.Responses.FlowStep;
using FlowManager.Shared.DTOs.Responses.FormTemplate;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using System.Text.Json;

namespace FlowManager.Client.Pages
{
    public partial class BasicUser : ComponentBase, IDisposable
    {
        private string _activeTab = "MYFORMS";
        protected string? errorMessage;

        // Form selection modal state
        private bool showFormSelectionModal = false;
        private bool showAddFormModal = false;
        private bool showCompleteOnBehalfModal = false;
        private bool isLoadingTemplates = false;
        private List<FormTemplateResponseDto>? displayedTemplates;

        // Search and pagination state for templates
        private string searchTerm = "";
        private int currentPage = 1;
        private const int pageSize = 8;
        private bool hasMoreTemplates = false;
        private int totalTemplatesCount = 0;
        private Timer? searchDebounceTimer;

        private Guid currentUserId = Guid.Empty;

        //View Form modal state
        private bool showViewFormModal = false;
        private bool isLoadingFormDetails = false;
        private bool isLoadingFlowSteps = false;
        private List<FlowStepResponseDto>? flowSteps;
        private FormResponseResponseDto? selectedFormResponse;
        private FormTemplateResponseDto? selectedFormTemplate;
        private List<ComponentResponseDto>? formComponents;
        private List<FormElement>? formElements;
        [Inject] private AuthService _authService { get; set; } = default!;
        private UserVM _currentUser = new();

        [Inject] protected FlowService FlowService { get; set; } = default!;
        [Inject] protected FormTemplateService FormTemplateService { get; set; } = default!;
        [Inject] protected FormResponseService FormResponseService { get; set; } = default!;
        [Inject] protected ComponentService ComponentService { get; set; } = default!;
        [Inject] protected NavigationManager Navigation { get; set; } = default!;
        [Inject] protected AuthenticationStateProvider AuthProvider { get; set; } = default!;
        [Inject] protected CookieAuthStateProvider CookieAuthStateProvider { get; set; } = default!;
        [Inject] protected HttpClient Http { get; set; } = default!;
        [Inject] protected IJSRuntime JSRuntime { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthProvider.GetAuthenticationStateAsync();

            if (!authState.User.Identity?.IsAuthenticated ?? true)
            {
                Navigation.NavigateTo("/");
                return;
            }

            if (!authState.User.IsInRole("Basic"))
            {
                Navigation.NavigateTo("/");
                return;
            }

            await LoadCurrentUser();

            await GetCurrentUser();
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

        private async Task ShowFormSelectionModalForAddForm()
        {
            showAddFormModal = true;
            showFormSelectionModal = true;
            searchTerm = "";
            currentPage = 1;
            displayedTemplates = new List<FormTemplateResponseDto>();
            hasMoreTemplates = false;
            totalTemplatesCount = 0;

            StateHasChanged();
            await LoadActiveTemplates();
        }

        private async Task ShowFormSelectionModalForCompleteOnBehalf()
        {
            showCompleteOnBehalfModal = true;
            showFormSelectionModal = true;
            searchTerm = "";
            currentPage = 1;
            displayedTemplates = new List<FormTemplateResponseDto>();
            hasMoreTemplates = false;
            totalTemplatesCount = 0;

            StateHasChanged();
            await LoadActiveTemplates();
        }

        private async Task LoadActiveTemplates(bool append = false)
        {
            isLoadingTemplates = true;
            StateHasChanged();

            try
            {
                Console.WriteLine($"[BasicUser] Loading active templates - Page: {currentPage}, Search: '{searchTerm}', Append: {append}");

                var response = await FormTemplateService.GetActiveFormTemplatesPagedAsync(
                    page: currentPage,
                    pageSize: pageSize,
                    searchTerm: string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm
                );

                if (response != null)
                {
                    if (append && displayedTemplates != null)
                    {
                        displayedTemplates.AddRange(response.Templates);
                    }
                    else
                    {
                        displayedTemplates = response.Templates.ToList();
                    }

                    hasMoreTemplates = response.HasMore;
                    totalTemplatesCount = response.TotalCount;

                    Console.WriteLine($"[BasicUser] Loaded {response.Templates.Count} active templates. Total: {totalTemplatesCount}, HasMore: {hasMoreTemplates}");
                }
                else
                {
                    Console.WriteLine("[BasicUser] No response received from FormTemplateService");
                    displayedTemplates = new List<FormTemplateResponseDto>();
                    hasMoreTemplates = false;
                    totalTemplatesCount = 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BasicUser] Error loading active templates: {ex.Message}");
                displayedTemplates = new List<FormTemplateResponseDto>();
                hasMoreTemplates = false;
                totalTemplatesCount = 0;
                await JSRuntime.InvokeVoidAsync("alert", $"Error loading form templates: {ex.Message}");
            }
            finally
            {
                isLoadingTemplates = false;
                StateHasChanged();
            }
        }

        private async Task LoadMoreTemplates()
        {
            if (isLoadingTemplates || !hasMoreTemplates) return;

            currentPage++;
            await LoadActiveTemplates(append: true);
        }

        private void OnSearchInput(ChangeEventArgs e)
        {
            var newSearchTerm = e.Value?.ToString() ?? "";

            searchDebounceTimer?.Dispose();
            searchDebounceTimer = new Timer(async _ =>
            {
                searchTerm = newSearchTerm;
                currentPage = 1;
                await InvokeAsync(async () =>
                {
                    await LoadActiveTemplates();
                });
            }, null, 500, Timeout.Infinite);
        }

        private async Task ClearSearch()
        {
            searchTerm = "";
            currentPage = 1;
            await LoadActiveTemplates();
        }

        private void CloseFormSelectionModal()
        {
            showFormSelectionModal = false;
            searchDebounceTimer?.Dispose();
            searchDebounceTimer = null;
            StateHasChanged();
        }

        private async Task SelectTemplate(FormTemplateResponseDto template)
        {
            if (showAddFormModal)
            {
                Navigation.NavigateTo($"/fill-form/{template.Id}");
            }
            else if (showCompleteOnBehalfModal)
            {
                Navigation.NavigateTo($"/complete-on-behalf-form/{template.Id}");
            }
        }

        private async Task ViewFormResponse(FormResponseResponseDto formResponse)
        {
            selectedFormResponse = formResponse;
            showViewFormModal = true;
            isLoadingFormDetails = true;
            isLoadingFlowSteps = true;
            StateHasChanged();

            try
            {
                selectedFormTemplate = await FormTemplateService.GetFormTemplateByIdAsync(formResponse.FormTemplateId);

                if (selectedFormTemplate != null)
                {
                    await ParseFormContent();

                    await LoadFormComponents();

                    await LoadFlowSteps();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading form details: {ex.Message}");
                await JSRuntime.InvokeVoidAsync("alert", $"Error loading form details: {ex.Message}");
            }
            finally
            {
                isLoadingFormDetails = false;
                isLoadingFlowSteps = false;
                StateHasChanged();
            }
        }

        private async Task LoadFlowSteps()
        {
            if (selectedFormResponse == null)
            {
                flowSteps = new List<FlowStepResponseDto>();
                return;
            }

            try
            {
                isLoadingFlowSteps = true;

                if (selectedFormTemplate?.FlowId != null)
                {
                    var flowStepsResponse = await Http.GetAsync($"api/flows/{selectedFormTemplate.FlowId}/steps");
                    if (flowStepsResponse.IsSuccessStatusCode)
                    {
                        var flowStepsApiResponse = await flowStepsResponse.Content.ReadFromJsonAsync<ApiResponse<List<FlowStepResponseDto>>>();
                        flowSteps = flowStepsApiResponse?.Result ?? new List<FlowStepResponseDto>();
                        return;
                    }
                }
                else
                {
                    flowSteps = new List<FlowStepResponseDto>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FlowVisualizer] Error loading flow steps: {ex.Message}");
                flowSteps = new List<FlowStepResponseDto>();
            }
            finally
            {
                isLoadingFlowSteps = false;
                StateHasChanged();
            }
        }

        private string GetFormStatus(FormResponseResponseDto form)
        {
            Console.WriteLine($"form response is approved : {form.IsApproved}");
            if (!string.IsNullOrEmpty(form.Status))
            {
                return form.Status;
            }

            if (!string.IsNullOrEmpty(form.RejectReason))
            {
                return "Rejected";
            }

            return "Pending";
        }

        private string GetStatusColor(string status)
        {
            return status switch
            {
                "Pending" => "#f59e0b", // galben
                "Rejected" => "#ef4444", // roșu
                "Approved" => "#10b981", // verde
                _ => "#6b7280" // gri default
            };
        }
        private async Task ParseFormContent()
        {
            if (string.IsNullOrEmpty(selectedFormTemplate?.Content))
                return;

            try
            {
                var contentData = JsonSerializer.Deserialize<FormContent>(selectedFormTemplate.Content);
                formElements = contentData?.Elements?.ToList() ?? new List<FormElement>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing form content: {ex.Message}");
                formElements = new List<FormElement>();
            }
        }

        private async Task LoadFormComponents()
        {
            if (selectedFormTemplate?.Components?.Any() != true)
                return;

            try
            {
                var componentTasks = selectedFormTemplate.Components.Select(async formTemplateComponent =>
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
                formComponents = componentResults.Where(c => c != null).ToList()!;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading components: {ex.Message}");
                formComponents = new List<ComponentResponseDto>();
            }
        }

        private void CloseViewFormModal()
        {
            showViewFormModal = false;
            selectedFormResponse = null;
            selectedFormTemplate = null;
            formComponents = null;
            formElements = null;
            flowSteps = null;
            StateHasChanged();
        }

        private object? GetFieldValue(Guid componentId)
        {
            if (selectedFormResponse?.ResponseFields?.ContainsKey(componentId) == true)
            {
                return selectedFormResponse.ResponseFields[componentId];
            }
            return null;
        }

        private void SetActiveTab(string tab)
        {
            _activeTab = tab;
        }

        protected async Task Logout()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/logout");
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

            var response = await Http.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("[Auth] Logout successful, notifying state provider");
                (CookieAuthStateProvider as CookieAuthStateProvider)?.NotifyUserLogout();
                Navigation.NavigateTo("/");
            }
            else
            {
                Console.WriteLine($"[Auth] Logout failed with status: {response.StatusCode}");
                errorMessage = "Logout failed. Please try again.";
            }
        }
        public void Dispose()
        {
            searchDebounceTimer?.Dispose();
        }

        private async Task GetCurrentUser()
        {
            UserProfileDto? result = await _authService.GetCurrentUserAsync();

            if (result == null)
                return;

            _currentUser = new UserVM
            {
                Id = result.Id,
                Name = result.Name,
                Email = result.Email
            };
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