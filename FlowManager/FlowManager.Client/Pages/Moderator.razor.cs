using BlazorBootstrap;
using FlowManager.Client.DTOs;
using FlowManager.Client.Deserialization;
using FlowManager.Client.Services;
using FlowManager.Client.ViewModels;
using FlowManager.Shared.DTOs;
using FlowManager.Shared.DTOs.Requests.FormResponse;
using FlowManager.Shared.DTOs.Responses;
using FlowManager.Shared.DTOs.Responses.Component;
using FlowManager.Shared.DTOs.Responses.Flow;
using FlowManager.Shared.DTOs.Responses.FormReview;
using FlowManager.Shared.DTOs.Responses.FormTemplate;
using FlowManager.Shared.DTOs.Responses.Step;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using System.Text.Json;
using static FlowManager.Client.Pages.FillForm;

namespace FlowManager.Client.Pages
{
    public partial class Moderator : ComponentBase, IDisposable
    {
        [Inject] private FormReviewService FormReviewService { get; set; } = default!;
        [Inject] private AuthService _authService { get; set; } = default!;

        private string _activeTab = "ASSIGNED";
        protected string? errorMessage;

        // Search and loading state
        private string searchTerm = "";
        private bool isLoading = false;
        private Timer? searchDebounceTimer;

        // Assigned forms data
        private List<FormResponseResponseDto>? assignedForms;
        private bool hasMoreForms = false;
        private int totalFormsCount = 0;
        private Guid currentModeratorId = Guid.Empty;

        // Pagination state for assigned forms
        private int _pageSize = 8;
        private int _currentPage = 1;
        private int _totalPages = 0;
        private int _maxVisiblePages = 4;
        private int _totalCount = 0;

        // View Form modal state
        private bool showViewFormModal = false;
        private bool isLoadingFormDetails = false;
        private FormResponseResponseDto? selectedFormResponse;
        private FormTemplateResponseDto? selectedFormTemplate;
        private List<ComponentResponseDto>? formComponents;
        private List<FormElement>? formElements;

        // Review state
        private bool isSubmittingReview = false;
        private bool showRejectModal = false;
        private string _rejectReason = "";
        private NextStepInfo? nextStepInfo;

        // History tab state
        private List<FormReviewResponseDto>? reviewHistory;
        private bool isLoadingHistory = false;
        private int _historyCurrentPage = 1;
        private int _historyPageSize = 8;
        private int _historyTotalPages = 0;
        private int _historyTotalCount = 0;
        private string historySearchTerm = "";

        // Status filter state for history (like BasicUser)
        private HashSet<string> selectedHistoryActions = new HashSet<string> { "Approved", "Rejected" };
        private const int _historyMaxVisiblePages = 5;

        private UserVM _currentUser = new();

        // Property pentru reject reason cu StateHasChanged
        private string rejectReason
        {
            get => _rejectReason;
            set
            {
                if (_rejectReason != value)
                {
                    _rejectReason = value;
                    StateHasChanged(); // Forțează re-render când se schimbă valoarea
                }
            }
        }

        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthProvider.GetAuthenticationStateAsync();

            if (!authState.User.Identity?.IsAuthenticated ?? true)
            {
                Navigation.NavigateTo("/");
                return;
            }

            if (!authState.User.IsInRole("Moderator"))
            {
                Console.WriteLine("[Moderator] User does not have Moderator role, redirecting to home");
                Navigation.NavigateTo("/");
                return;
            }

            await LoadCurrentModerator();
            if (currentModeratorId != Guid.Empty)
            {
                await LoadAssignedForms();
            }

            Console.WriteLine("[Moderator] User has Moderator role, allowing access");

            await GetCurrentUser();
        }

        private async Task LoadCurrentModerator()
        {
            try
            {
                var response = await Http.GetAsync("api/auth/me");
                if (response.IsSuccessStatusCode)
                {
                    var userInfo = await response.Content.ReadFromJsonAsync<UserProfileDto>();
                    if (userInfo != null && userInfo.Id != Guid.Empty)
                    {
                        currentModeratorId = userInfo.Id;
                        Console.WriteLine($"[DEBUG] Current moderator ID: {currentModeratorId}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to load current moderator: {ex.Message}");
            }
        }

        private void SetActiveTab(string tab)
        {
            _activeTab = tab;

            if (tab == "HISTORY" && currentModeratorId != Guid.Empty)
            {
                _ = LoadReviewHistory();
            }
        }

        private async Task LoadReviewHistory(bool append = false)
        {
            isLoadingHistory = true;
            StateHasChanged();

            try
            {
                var response = await FormReviewService.GetReviewHistoryByModeratorAsync(
                    currentModeratorId,
                    _historyCurrentPage,
                    _historyPageSize,
                    string.IsNullOrEmpty(historySearchTerm) ? null : historySearchTerm,
                    selectedHistoryActions.Count == 1 ? selectedHistoryActions.First() : null);

                if (response != null)
                {
                    if (append && reviewHistory != null)
                    {
                        reviewHistory.AddRange(response.Reviews);
                    }
                    else
                    {
                        reviewHistory = response.Reviews;
                    }

                    _historyTotalCount = response.TotalCount;
                    _historyTotalPages = (int)Math.Ceiling((double)_historyTotalCount / _historyPageSize);

                    Console.WriteLine($"[Moderator] Loaded {response.Reviews.Count} reviews, total: {_historyTotalCount}");
                }
                else
                {
                    reviewHistory = new List<FormReviewResponseDto>();
                    _historyTotalCount = 0;
                    _historyTotalPages = 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Moderator] Error loading review history: {ex.Message}");
                reviewHistory = new List<FormReviewResponseDto>();
            }
            finally
            {
                isLoadingHistory = false;
                StateHasChanged();
            }
        }

        private async Task RefreshReviewHistory()
        {
            historySearchTerm = "";
            selectedHistoryActions = new HashSet<string> { "Approved", "Rejected" };
            _historyCurrentPage = 1;
            await LoadReviewHistory();
        }

        private void OnHistorySearchInput(ChangeEventArgs e)
        {
            var newSearchTerm = e.Value?.ToString() ?? "";
            historySearchTerm = newSearchTerm;

            // Debounce search
            searchDebounceTimer?.Dispose();
            searchDebounceTimer = new Timer(async _ =>
            {
                await InvokeAsync(async () =>
                {
                    _historyCurrentPage = 1;
                    await LoadReviewHistory();
                });
            }, null, 500, Timeout.Infinite);
        }

        // History status filter methods (like BasicUser)
        private async Task FilterHistoryByAction(string action)
        {
            selectedHistoryActions = new HashSet<string> { action };
            _historyCurrentPage = 1;
            await LoadReviewHistory();
        }

        private async Task ShowAllHistoryActions()
        {
            selectedHistoryActions = new HashSet<string> { "Approved", "Rejected" };
            _historyCurrentPage = 1;
            await LoadReviewHistory();
        }

        private bool IsOnlyHistoryActionSelected(string action)
        {
            return selectedHistoryActions.Count == 1 && selectedHistoryActions.Contains(action);
        }

        private bool AreAllHistoryActionsSelected()
        {
            return selectedHistoryActions.Count == 2 &&
                   selectedHistoryActions.Contains("Approved") &&
                   selectedHistoryActions.Contains("Rejected");
        }

        private async Task ClearHistorySearch()
        {
            historySearchTerm = "";
            selectedHistoryActions = new HashSet<string> { "Approved", "Rejected" };
            _historyCurrentPage = 1;
            await LoadReviewHistory();
        }

        private async Task GoToHistoryFirstPage()
        {
            _historyCurrentPage = 1;
            await LoadReviewHistory();
        }

        private async Task GoToHistoryPreviousPage()
        {
            _historyCurrentPage--;
            await LoadReviewHistory();
        }

        private async Task GoToHistoryPage(int page)
        {
            _historyCurrentPage = page;
            await LoadReviewHistory();
        }

        private async Task GoToHistoryNextPage()
        {
            _historyCurrentPage++;
            await LoadReviewHistory();
        }

        private async Task GoToHistoryLastPage()
        {
            _historyCurrentPage = _historyTotalPages;
            await LoadReviewHistory();
        }

        // History pagination helper methods (like BasicUser)
        private IEnumerable<int> GetHistoryPageNumbers()
        {
            var startPage = Math.Max(1, _historyCurrentPage - _historyMaxVisiblePages / 2);
            var endPage = Math.Min(_historyTotalPages, startPage + _historyMaxVisiblePages - 1);

            // Adjust start if we're near the end
            if (endPage - startPage < _historyMaxVisiblePages - 1)
            {
                startPage = Math.Max(1, endPage - _historyMaxVisiblePages + 1);
            }

            return Enumerable.Range(startPage, endPage - startPage + 1);
        }



        private async Task LoadAssignedForms(bool append = false)
        {
            isLoading = true;
            StateHasChanged();

            try
            {
                var payload = new QueriedFormResponseRequestDto
                {
                    IncludeDeleted = false,
                    QueryParams = new Shared.DTOs.Requests.QueryParamsDto
                    {
                        Page = _currentPage,
                        PageSize = _pageSize,
                        SortBy = "CreatedAt",
                        SortDescending = true
                    }
                };

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    payload.SearchTerm = searchTerm;
                }

                var response = await FormResponseService.GetFormResponsesAssignedToModeratorAsync(
                    currentModeratorId,
                    _currentPage,
                    _pageSize,
                    searchTerm);

                if (response != null)
                {
                    if (append && assignedForms != null)
                    {
                        assignedForms.AddRange(response.FormResponses);
                    }
                    else
                    {
                        assignedForms = response.FormResponses;
                    }

                    _totalCount = response.TotalCount;
                    _totalPages = (int)Math.Ceiling((double)_totalCount / _pageSize);
                    hasMoreForms = response.HasMore;
                    totalFormsCount = response.TotalCount;

                    Console.WriteLine($"[Moderator] Loaded {assignedForms?.Count ?? 0} assigned forms (page {_currentPage}/{_totalPages})");
                }
                else
                {
                    assignedForms = new List<FormResponseResponseDto>();
                    _totalCount = 0;
                    _totalPages = 0;
                    hasMoreForms = false;
                    totalFormsCount = 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Moderator] Error loading assigned forms: {ex.Message}");
                assignedForms = new List<FormResponseResponseDto>();
                _totalCount = 0;
                _totalPages = 0;
                hasMoreForms = false;
                totalFormsCount = 0;
            }
            finally
            {
                isLoading = false;
                StateHasChanged();
            }
        }

        private async Task RefreshAssignedForms()
        {
            searchTerm = "";
            _currentPage = 1;
            await LoadAssignedForms();
        }

        private void OnSearchInput(ChangeEventArgs e)
        {
            var newSearchTerm = e.Value?.ToString() ?? "";

            // Debounce search to avoid too many API calls
            searchDebounceTimer?.Dispose();
            searchDebounceTimer = new Timer(async _ =>
            {
                await InvokeAsync(async () =>
                {
                    searchTerm = newSearchTerm;
                    _currentPage = 1; // Reset la prima pagină la search nou
                    await LoadAssignedForms();
                });
            }, null, 500, Timeout.Infinite); // 500ms debounce
        }

        private async Task GoToFirstPage()
        {
            _currentPage = 1;
            await LoadAssignedForms();
        }

        private async Task GoToPreviousPage()
        {
            _currentPage--;
            await LoadAssignedForms();
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
            await LoadAssignedForms();
        }

        private async Task GoToNextPage()
        {
            _currentPage++;
            await LoadAssignedForms();
        }

        private async Task GoToLastPage()
        {
            _currentPage = _totalPages;
            await LoadAssignedForms();
        }

        private async Task ClearSearch()
        {
            searchTerm = "";
            _currentPage = 1;
            await LoadAssignedForms();
        }

        private async Task ViewFormResponse(FormResponseResponseDto formResponse)
        {
            selectedFormResponse = formResponse;
            showViewFormModal = true;
            isLoadingFormDetails = true;
            StateHasChanged();

            try
            {
                selectedFormTemplate = await FormTemplateService.GetFormTemplateByIdAsync(formResponse.FormTemplateId);

                if (selectedFormTemplate != null)
                {
                    await ParseFormContent();
                    await LoadFormComponents();
                    await LoadNextStepInfo();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading form details: {ex.Message}");
            }
            finally
            {
                isLoadingFormDetails = false;
                StateHasChanged();
            }
        }

        private async Task LoadNextStepInfo()
        {
            if (selectedFormResponse == null || selectedFormTemplate == null) return;

            try
            {
                // Găsește flow-ul pentru acest form template
                var flowsResponse = await Http.GetAsync($"api/flows/queried?QueryParams.PageSize=100");
                if (flowsResponse.IsSuccessStatusCode)
                {
                    var flowsData = await flowsResponse.Content.ReadFromJsonAsync<ApiResponse<PagedResponseDto<FlowResponseDto>>>();
                    var flows = flowsData?.Result?.Data;

                    Console.WriteLine($"[DEBUG STEPS] Found {flows?.Count() ?? 0} flows total");

                    if (flows != null)
                    {
                        var associatedFlow = flows.FirstOrDefault(f => f.FormTemplateId == selectedFormTemplate.Id);
                        Console.WriteLine($"[DEBUG STEPS] Associated flow: {associatedFlow?.Name ?? "NOT FOUND"}");
                        Console.WriteLine($"[DEBUG STEPS] Flow has {associatedFlow?.FlowSteps?.Count() ?? 0} steps");

                        if (associatedFlow?.FlowSteps?.Any() == true)
                        {
                            var orderedSteps = associatedFlow.FlowSteps.ToList();
                            Console.WriteLine($"[DEBUG STEPS] Steps in order:");

                            var currentStepIndex = orderedSteps.FindIndex(fs => fs.Id == selectedFormResponse.Id);
                            Console.WriteLine($"[DEBUG STEPS] Current step index: {currentStepIndex}");

                            if (currentStepIndex >= 0)
                            {
                                var hasNextStep = currentStepIndex < orderedSteps.Count - 1;
                                Console.WriteLine($"[DEBUG STEPS] Has next step: {hasNextStep}");


                                nextStepInfo = new NextStepInfo
                                {
                                    HasNextStep = hasNextStep,
                                    NextStepName = hasNextStep ? orderedSteps[currentStepIndex + 1].FlowStepItems.First().Step.StepName : null,
                                    NextStepId = hasNextStep ? orderedSteps[currentStepIndex + 1].FlowStepItems.First().Step.StepId : null
                                };

                                Console.WriteLine($"[DEBUG STEPS] NextStepInfo created: HasNext={nextStepInfo.HasNextStep}, NextName={nextStepInfo.NextStepName}, NextId={nextStepInfo.NextStepId}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading next step info: {ex.Message}");
            }
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

        private void CloseViewFormModal()
        {
            showViewFormModal = false;
            selectedFormResponse = null;
            selectedFormTemplate = null;
            formComponents = null;
            formElements = null;
            nextStepInfo = null;
            StateHasChanged();
        }

        private void ShowRejectModal()
        {
            rejectReason = "";
            showRejectModal = true;
            showViewFormModal = false;
            StateHasChanged();
        }

        private void CloseRejectModal()
        {
            showRejectModal = false;
            rejectReason = "";
            StateHasChanged();
        }

        private void OnRejectReasonInput(ChangeEventArgs e)
        {
            rejectReason = e.Value?.ToString() ?? "";
        }

        private async Task ApproveForm()
        {
            if (selectedFormResponse == null || isSubmittingReview) return;

            isSubmittingReview = true;
            StateHasChanged();

            try
            {
                var payload = new PatchFormResponseRequestDto
                {
                    Id = selectedFormResponse.Id,
                    ReviewerId = currentModeratorId
                };

                Console.WriteLine($"[DEBUG APPROVE] Form template: {selectedFormResponse.FormTemplateName}");
                Console.WriteLine($"[DEBUG APPROVE] Next step exists: {nextStepInfo?.HasNextStep}");
                Console.WriteLine($"[DEBUG APPROVE] Next step ID: {nextStepInfo?.NextStepId}");
                Console.WriteLine($"[DEBUG APPROVE] Next step Name: {nextStepInfo?.NextStepName}");

                if (nextStepInfo?.HasNextStep == true && nextStepInfo.NextStepId != Guid.Empty)
                {
                    Console.WriteLine($"[DEBUG APPROVE] MOVING TO NEXT STEP: {nextStepInfo.NextStepId.Value}");

                    var response = await Http.PatchAsJsonAsync($"api/formresponses/{selectedFormResponse.Id}", payload);

                    var responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[DEBUG APPROVE] Server response status: {response.StatusCode}");
                    Console.WriteLine($"[DEBUG APPROVE] Server response content: {responseContent}");

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("[Success] Form approved and moved to next step successfully!");
                        CloseViewFormModal();
                        await LoadAssignedForms();
                        if (_activeTab == "HISTORY")
                        {
                            await LoadReviewHistory();
                        }
                    }
                    else
                    {
                        Console.WriteLine("[Error] Failed to approve form. Please try again.");
                    }
                }
                else
                {
                    payload.Status = "Approved";
                    Console.WriteLine($"[DEBUG APPROVE] FINAL APPROVAL - setting Status to Approved");

                    var response = await Http.PatchAsJsonAsync($"api/formresponses/{selectedFormResponse.Id}", payload);

                    var responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[DEBUG APPROVE] Server response status: {response.StatusCode}");
                    Console.WriteLine($"[DEBUG APPROVE] Server response content: {responseContent}");

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("[Success] Form approved successfully!");
                        CloseViewFormModal();
                        await LoadAssignedForms();
                        // ACTUALIZEAZĂ și istoricul dacă e deschis
                        if (_activeTab == "HISTORY")
                        {
                            await LoadReviewHistory();
                        }
                    }
                    else
                    {
                        Console.WriteLine("[Error] Failed to approve form. Please try again.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error approving form: {ex.Message}");
            }
            finally
            {
                isSubmittingReview = false;
                StateHasChanged();
            }
        }

        private async Task ConfirmReject()
        {
            if (selectedFormResponse == null || string.IsNullOrWhiteSpace(rejectReason) || isSubmittingReview)
                return;

            isSubmittingReview = true;
            StateHasChanged();

            try
            {
                var payload = new PatchFormResponseRequestDto
                {
                    Id = selectedFormResponse.Id,
                    RejectReason = rejectReason.Trim(),
                    ReviewerId = currentModeratorId // ADAUGĂ ID-ul moderatorului
                };

                var response = await Http.PatchAsJsonAsync($"api/formresponses/{selectedFormResponse.Id}", payload);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("[Success] Form rejected successfully!");
                    CloseRejectModal();
                    CloseViewFormModal();
                    await LoadAssignedForms();
                    // ACTUALIZEAZĂ și istoricul dacă e deschis
                    if (_activeTab == "HISTORY")
                    {
                        await LoadReviewHistory();
                    }
                }
                else
                {
                    Console.WriteLine("[Error] Failed to reject form. Please try again.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error rejecting form: {ex.Message}");
            }
            finally
            {
                isSubmittingReview = false;
                StateHasChanged();
            }
        }

        private string GetFormStatus(FormResponseResponseDto form)
        {
            // Folosește statusul din baza de date direct
            if (!string.IsNullOrEmpty(form.Status))
            {
                return form.Status;
            }

            // Fallback pentru compatibilitate
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

        private bool CanReviewForm(FormResponseResponseDto form)
        {
            var status = GetFormStatus(form);
            // Permite review doar pentru formularele Pending
            return status == "Pending";
        }

        private bool ShouldShowReviewButtons(FormResponseResponseDto form)
        {
            // Arată butoanele doar pentru formularele care pot fi reviewed
            return CanReviewForm(form);
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

        private async Task ViewFormResponseFromReview(FormReviewResponseDto review)
        {
            try
            {
                // Try to find the form in the currently loaded assigned forms first
                var formResponse = assignedForms?.FirstOrDefault(f => f.Id == review.FormResponseId);
                
                if (formResponse != null)
                {
                    // Form is still assigned - use the existing ViewFormResponse method
                    await ViewFormResponse(formResponse);
                }
                else
                {
                    // Form is no longer assigned - create a FormResponseResponseDto from review info
                    var reviewFormResponse = new FormResponseResponseDto
                    {
                        Id = review.FormResponseId,
                        FormTemplateId = review.FormTemplateId,
                        FormTemplateName = review.FormTemplateName,
                        ResponseFields = review.ResponseFields,
                        UserName = review.UserName,
                        UserEmail = review.UserEmail,
                        Status = "Reviewed",
                        CreatedAt = review.CreatedAt ?? DateTime.UtcNow,
                        UpdatedAt = review.UpdatedAt,
                        DeletedAt = review.DeletedAt
                    };

                    await ViewFormResponse(reviewFormResponse);
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Error loading form response: {ex.Message}";
                StateHasChanged();
            }
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
    }
}