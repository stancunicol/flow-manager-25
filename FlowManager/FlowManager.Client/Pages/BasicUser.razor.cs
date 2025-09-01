using FlowManager.Client.DTOs;
using FlowManager.Client.Services;
using FlowManager.Shared.DTOs;
using FlowManager.Shared.DTOs.Requests.FormResponse;
using FlowManager.Shared.DTOs.Responses;
using FlowManager.Shared.DTOs.Responses.Component;
using FlowManager.Shared.DTOs.Responses.Flow;
using FlowManager.Shared.DTOs.Responses.FormTemplate;
using FlowManager.Shared.DTOs.Responses.Step;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using static FlowManager.Client.Pages.FillForm;

namespace FlowManager.Client.Pages
{
    public partial class BasicUser : ComponentBase, IDisposable
    {
        private string _activeTab = "MYFORMS";
        protected string? errorMessage;

        // Form selection modal state
        private bool showFormSelectionModal = false;
        private bool isLoadingTemplates = false;
        private List<FormTemplateResponseDto>? displayedTemplates;

        // Search and pagination state for templates
        private string searchTerm = "";
        private int currentPage = 1;
        private const int pageSize = 8;
        private bool hasMoreTemplates = false;
        private int totalTemplatesCount = 0;
        private Timer? searchDebounceTimer;

        // User forms state with search
        private bool isLoadingUserForms = false;
        private List<FormResponseResponseDto>? userForms;
        private List<FormResponseResponseDto>? filteredUserForms;
        private string userFormsSearchTerm = "";
        private Timer? userFormsSearchDebounceTimer;
        private Guid currentUserId = Guid.Empty;

        // Pagination state for user forms
        private int _pageSize = 8; 
        private int _currentPage = 1;
        private int _totalPages = 0;
        private int _maxVisiblePages = 4;
        private int _totalCount = 0;

        //View Form modal state
        private bool showViewFormModal = false;
        private bool isLoadingFormDetails = false;
        private bool isLoadingFlowSteps = false;
        private List<StepResponseDto>? flowSteps;
        private FormResponseResponseDto? selectedFormResponse;
        private FormTemplateResponseDto? selectedFormTemplate;
        private List<ComponentResponseDto>? formComponents;
        private List<FormElement>? formElements;

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
            if (currentUserId != Guid.Empty)
            {
                await LoadUserForms();
            }
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

        private async Task LoadUserForms()
        {
            isLoadingUserForms = true;
            StateHasChanged();

            try
            {
                var payload = new QueriedFormResponseRequestDto
                {
                    UserId = currentUserId,
                    IncludeDeleted = false,
                    QueryParams = new Shared.DTOs.Requests.QueryParamsDto
                    {
                        Page = _currentPage,
                        PageSize = _pageSize,
                        SortBy = "CreatedAt",
                        SortDescending = true
                    }
                };

                if (!string.IsNullOrEmpty(userFormsSearchTerm))
                {
                    payload.SearchTerm = userFormsSearchTerm;
                }

                var response = await FormResponseService.GetFormResponsesByUserPagedAsync(
                    currentUserId,
                    _currentPage,
                    _pageSize,
                    userFormsSearchTerm);

                if (response != null)
                {
                    userForms = response.FormResponses;
                    _totalCount = response.TotalCount;
                    _totalPages = (int)Math.Ceiling((double)_totalCount / _pageSize);

                    Console.WriteLine($"Loaded {userForms?.Count ?? 0} forms for user {currentUserId} (page {_currentPage}/{_totalPages})");
                }
                else
                {
                    userForms = new List<FormResponseResponseDto>();
                    _totalCount = 0;
                    _totalPages = 0;
                }

                // Nu mai e nevoie de ApplyUserFormsFilter() pentru că search-ul se face server-side
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading user forms: {ex.Message}");
                userForms = new List<FormResponseResponseDto>();
                _totalCount = 0;
                _totalPages = 0;
            }
            finally
            {
                isLoadingUserForms = false;
                StateHasChanged();
            }
        }

        // USER FORMS SEARCH FUNCTIONALITY
        private void OnUserFormsSearchInput(ChangeEventArgs e)
        {
            var newSearchTerm = e.Value?.ToString() ?? "";

            // Debounce search to avoid too many API calls
            userFormsSearchDebounceTimer?.Dispose();
            userFormsSearchDebounceTimer = new Timer(async _ =>
            {
                await InvokeAsync(async () =>
                {
                    userFormsSearchTerm = newSearchTerm;
                    _currentPage = 1; // Reset la prima pagină la search nou
                    await LoadUserForms();
                });
            }, null, 500, Timeout.Infinite); // 500ms debounce
        }

        private async Task ClearUserFormsSearch()
        {
            userFormsSearchTerm = "";
            _currentPage = 1;
            await LoadUserForms();
        }

        private async Task GoToFirstPage()
        {
            _currentPage = 1;
            await LoadUserForms();
        }

        private async Task GoToPreviousPage()
        {
            _currentPage--;
            await LoadUserForms();
        }

        private List<int> GetPageNumbers()
        {
            List<int> pages = new List<int>();

            int half = (int)Math.Floor(_maxVisiblePages / 2.0);

            int start = Math.Max(1, _currentPage - half);
            int end = Math.Min(_totalPages, start + _maxVisiblePages - 1);

            if (end - start + 1 < _maxVisiblePages)
            {
                start = Math.Max(1, end - _maxVisiblePages + 1);
            }

            for (int i = start; i <= end; i++)
            {
                pages.Add(i);
            }

            return pages;
        }

        private async Task GoToPage(int page)
        {
            _currentPage = page;
            await LoadUserForms();
        }

        private async Task GoToNextPage()
        {
            _currentPage++;
            await LoadUserForms();
        }

        private async Task GoToLastPage()
        {
            _currentPage = _totalPages;
            await LoadUserForms();
        }

        // FORM TEMPLATE SELECTION MODAL FUNCTIONALITY
        private async Task ShowFormSelectionModal()
        {
            showFormSelectionModal = true;
            searchTerm = "";
            currentPage = 1;
            displayedTemplates = new List<FormTemplateResponseDto>();
            hasMoreTemplates = false;
            totalTemplatesCount = 0;

            StateHasChanged();
            await LoadTemplates();
        }

        private async Task LoadTemplates(bool append = false)
        {
            isLoadingTemplates = true;
            StateHasChanged();

            try
            {
                Console.WriteLine($"Loading templates - Page: {currentPage}, Search: '{searchTerm}', Append: {append}");

                var response = await FormTemplateService.GetFormTemplatesPagedAsync(
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

                    Console.WriteLine($"Loaded {response.Templates.Count} templates. Total: {totalTemplatesCount}, HasMore: {hasMoreTemplates}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading templates: {ex.Message}");
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
            await LoadTemplates(append: true);
        }

        private void OnSearchInput(ChangeEventArgs e)
        {
            var newSearchTerm = e.Value?.ToString() ?? "";

            // Debounce search to avoid too many API calls
            searchDebounceTimer?.Dispose();
            searchDebounceTimer = new Timer(async _ =>
            {
                searchTerm = newSearchTerm;
                currentPage = 1;
                await InvokeAsync(async () =>
                {
                    await LoadTemplates();
                });
            }, null, 500, Timeout.Infinite); // 500ms debounce
        }

        private async Task ClearSearch()
        {
            searchTerm = "";
            currentPage = 1;
            await LoadTemplates();
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
            Navigation.NavigateTo($"/fill-form/{template.Id}");
        }

        private async Task ViewFormResponse(FormResponseResponseDto formResponse)
        {
            selectedFormResponse = formResponse;
            showViewFormModal = true;
            isLoadingFormDetails = true;
            isLoadingFlowSteps = true; // ADAUGĂ
            StateHasChanged();

            try
            {
                // Încarcă template-ul formularului
                selectedFormTemplate = await FormTemplateService.GetFormTemplateByIdAsync(formResponse.FormTemplateId);

                if (selectedFormTemplate != null)
                {
                    // Parsează conținutul formularului
                    await ParseFormContent();

                    // Încarcă componentele
                    await LoadFormComponents();

                    // ADAUGĂ - Încarcă step-urile flow-ului
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
                isLoadingFlowSteps = false; // ADAUGĂ
                StateHasChanged();
            }
        }

        private async Task LoadFlowSteps()
        {
            if (selectedFormResponse == null)
            {
                flowSteps = new List<StepResponseDto>();
                return;
            }

            try
            {
                isLoadingFlowSteps = true;
                Console.WriteLine($"[FlowVisualizer] Loading steps for form response step: {selectedFormResponse.StepId}");

                var stepResponse = await Http.GetAsync($"api/steps/{selectedFormResponse.StepId}");

                if (stepResponse.IsSuccessStatusCode)
                {
                    var stepApiResponse = await stepResponse.Content.ReadFromJsonAsync<ApiResponse<StepResponseDto>>();
                    var step = stepApiResponse?.Result;

                    if (step != null)
                    {
                        Console.WriteLine($"[FlowVisualizer] Found step: {step.Name}");

                        if (selectedFormTemplate != null)
                        {
                            var flowsResponse = await Http.GetAsync("api/flows/queried?QueryParams.PageSize=100");
                            if (flowsResponse.IsSuccessStatusCode)
                            {
                                var flowsApiResponse = await flowsResponse.Content.ReadFromJsonAsync<ApiResponse<PagedResponseDto<FlowResponseDto>>>();
                                var flows = flowsApiResponse?.Result?.Data;

                                if (flows?.Any() == true)
                                {
                                    var matchingFlow = flows.FirstOrDefault(f => f.Steps?.Any(s => s.Id == selectedFormResponse.StepId) == true);

                                    if (matchingFlow != null)
                                    {
                                        flowSteps = matchingFlow.Steps?.OrderBy(s => s.CreatedAt).ToList() ?? new List<StepResponseDto>();
                                        Console.WriteLine($"[FlowVisualizer] Loaded {flowSteps.Count} steps from flow: {matchingFlow.Name}");
                                    }
                                    else
                                    {
                                        Console.WriteLine("[FlowVisualizer] No matching flow found for this step");
                                        flowSteps = new List<StepResponseDto> { step };
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"[FlowVisualizer] Failed to load step info: {stepResponse.StatusCode}");
                    flowSteps = new List<StepResponseDto>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FlowVisualizer] Error loading flow steps: {ex.Message}");
                flowSteps = new List<StepResponseDto>();
            }
            finally
            {
                isLoadingFlowSteps = false;
                StateHasChanged();
            }
        }

        private string GetFormStatus(FormResponseResponseDto form)
        {
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

        private string FormatFieldValue(object? value, string componentType)
        {
            if (value == null) return "No response";

            return componentType.ToLower() switch
            {
                "checkbox" => value.ToString() == "True" ? "Yes" : "No",
                "datetime" => DateTime.TryParse(value.ToString(), out var date) ? date.ToString("dd/MM/yyyy HH:mm") : value.ToString()!,
                _ => value.ToString() ?? "No response"
            };
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

        private void AddForm()
        {
            _ = ShowFormSelectionModal();
        }

        private async Task RefreshUserForms()
        {
            Console.WriteLine("[BasicUser] Refreshing user forms...");
            _currentPage = 1;
            await LoadUserForms();
        }

        // Implementează IDisposable
        public void Dispose()
        {
            searchDebounceTimer?.Dispose();
            userFormsSearchDebounceTimer?.Dispose();
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