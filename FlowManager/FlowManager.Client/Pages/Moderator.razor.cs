using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using FlowManager.Client.Services;
using FlowManager.Shared.DTOs.Requests.FormResponse;
using FlowManager.Shared.DTOs;
using System.Net.Http.Json;

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
                // TODO: Load form details for viewing
                await Task.Delay(500); // Simulate loading
                Console.WriteLine($"[Moderator] Viewing form response: {formResponse.Id}");
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

        private void CloseViewFormModal()
        {
            showViewFormModal = false;
            selectedFormResponse = null;
            StateHasChanged();
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
    }
}