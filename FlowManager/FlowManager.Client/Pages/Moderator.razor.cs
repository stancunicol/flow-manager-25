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
using System.Text.Json;
using static FlowManager.Client.Pages.FillForm;

namespace FlowManager.Client.Pages
{
    public partial class Moderator : ComponentBase, IDisposable
    {
        private string _activeTab = "ASSIGNED";
        protected string? errorMessage;

        // Search and loading state
        private string searchTerm = "";
        private bool isLoading = false;
        private Timer? searchDebounceTimer;

        // Assigned forms data
        private List<FormResponseResponseDto>? assignedForms;
        private int currentPage = 1;
        private const int pageSize = 12;
        private bool hasMoreForms = false;
        private int totalFormsCount = 0;
        private Guid currentModeratorId = Guid.Empty;

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
        }

        private async Task LoadAssignedForms(bool append = false)
        {
            isLoading = true;
            StateHasChanged();

            try
            {
                Console.WriteLine($"Loading assigned forms - Page: {currentPage}, Search: '{searchTerm}', Append: {append}");

                var response = await FormResponseService.GetFormResponsesAssignedToModeratorAsync(
                    moderatorId: currentModeratorId,
                    page: currentPage,
                    pageSize: pageSize,
                    searchTerm: string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm
                );

                if (response != null)
                {
                    if (append && assignedForms != null)
                    {
                        assignedForms.AddRange(response.FormResponses);
                    }
                    else
                    {
                        assignedForms = response.FormResponses.ToList();
                    }

                    hasMoreForms = response.HasMore;
                    totalFormsCount = response.TotalCount;

                    Console.WriteLine($"Loaded {response.FormResponses.Count} assigned forms. Total: {totalFormsCount}, HasMore: {hasMoreForms}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Moderator] Error loading assigned forms: {ex.Message}");
            }
            finally
            {
                isLoading = false;
                StateHasChanged();
            }
        }

        private async Task LoadMoreForms()
        {
            if (isLoading || !hasMoreForms) return;

            currentPage++;
            await LoadAssignedForms(append: true);
        }

        private async Task RefreshAssignedForms()
        {
            searchTerm = "";
            currentPage = 1;
            await LoadAssignedForms();
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
                    await LoadAssignedForms();
                });
            }, null, 500, Timeout.Infinite); // 500ms debounce
        }

        private async Task ClearSearch()
        {
            searchTerm = "";
            currentPage = 1;
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
                // Încarcă template-ul formularului
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

                    if (flows != null)
                    {
                        var associatedFlow = flows.FirstOrDefault(f => f.FormTemplateId == selectedFormTemplate.Id);

                        if (associatedFlow?.Steps?.Any() == true)
                        {
                            var orderedSteps = associatedFlow.Steps.ToList();
                            var currentStepIndex = orderedSteps.FindIndex(s => s.Id == selectedFormResponse.StepId);

                            if (currentStepIndex >= 0)
                            {
                                var hasNextStep = currentStepIndex < orderedSteps.Count - 1;
                                nextStepInfo = new NextStepInfo
                                {
                                    HasNextStep = hasNextStep,
                                    NextStepName = hasNextStep ? orderedSteps[currentStepIndex + 1].Name : null,
                                    NextStepId = hasNextStep ? orderedSteps[currentStepIndex + 1].Id : null
                                };
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

        // Review Actions
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

        // Metodă pentru actualizarea reject reason în timp real
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
                var updateData = new PatchFormResponseRequestDto
                {
                    Id = selectedFormResponse.Id,
                    RejectReason = null // Clear any existing reject reason
                };

                bool isFinalApproval = false;

                if (nextStepInfo?.HasNextStep == true && nextStepInfo.NextStepId.HasValue)
                {
                    // Move to next step - backend will set status to "Pending"
                    updateData.StepId = nextStepInfo.NextStepId.Value;
                    // Nu setăm Status explicit, să lase backend-ul să decidă
                }
                else
                {
                    // Final approval - backend will set status to "Approved"
                    updateData.StepId = selectedFormResponse.StepId; // Keep same step
                    updateData.Status = "Approved"; // Force final approval status
                    isFinalApproval = true;
                }

                var response = await Http.PatchAsJsonAsync($"api/formresponses/{selectedFormResponse.Id}", updateData);

                if (response.IsSuccessStatusCode)
                {
                    var successMessage = nextStepInfo?.HasNextStep == true
                        ? $"Form approved and moved to {nextStepInfo.NextStepName}"
                        : "Form approved successfully!";

                    await JSRuntime.InvokeVoidAsync("alert", successMessage);

                    // Elimină din lista locală doar dacă e final approval sau s-a mutat la alt step
                    if (isFinalApproval || nextStepInfo?.HasNextStep == true)
                    {
                        if (assignedForms != null)
                        {
                            assignedForms.RemoveAll(f => f.Id == selectedFormResponse.Id);
                            totalFormsCount = Math.Max(0, totalFormsCount - 1);
                        }
                    }

                    CloseViewFormModal();
                    StateHasChanged();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[ERROR] Approve failed: {response.StatusCode} - {errorContent}");
                    await JSRuntime.InvokeVoidAsync("alert", "Error approving form. Please try again.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Exception during approve: {ex.Message}");
                await JSRuntime.InvokeVoidAsync("alert", "Error approving form. Please try again.");
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
                var updateData = new PatchFormResponseRequestDto
                {
                    Id = selectedFormResponse.Id,
                    RejectReason = rejectReason.Trim()
                    // Nu setăm Status explicit - backend-ul va seta automat la "Rejected"
                };

                var response = await Http.PatchAsJsonAsync($"api/formresponses/{selectedFormResponse.Id}", updateData);

                if (response.IsSuccessStatusCode)
                {
                    await JSRuntime.InvokeVoidAsync("alert", "Form rejected successfully. The user will be notified.");

                    // Elimină din lista locală pentru feedback imediat
                    if (assignedForms != null)
                    {
                        assignedForms.RemoveAll(f => f.Id == selectedFormResponse.Id);
                        totalFormsCount = Math.Max(0, totalFormsCount - 1);
                    }

                    showRejectModal = false;
                    showViewFormModal = false;
                    CloseViewFormModal();
                    StateHasChanged();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[ERROR] Reject failed: {response.StatusCode} - {errorContent}");
                    await JSRuntime.InvokeVoidAsync("alert", "Error rejecting form. Please try again.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Exception during reject: {ex.Message}");
                await JSRuntime.InvokeVoidAsync("alert", "Error rejecting form. Please try again.");
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

        // Helper classes
        public class NextStepInfo
        {
            public bool HasNextStep { get; set; }
            public string? NextStepName { get; set; }
            public Guid? NextStepId { get; set; }
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