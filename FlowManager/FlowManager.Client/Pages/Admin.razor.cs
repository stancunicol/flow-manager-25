using FlowManager.Client.DTOs;
using FlowManager.Client.Services;
using FlowManager.Client.ViewModels;
using FlowManager.Shared.DTOs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using static System.Net.WebRequestMethods;

namespace FlowManager.Client.Pages
{
    public partial class Admin : ComponentBase, IDisposable
    {
        private string _activeTab = "USERS";
        protected string? errorMessage;
        [Inject] private AuthService _authService { get; set; } = default!;
        [Inject] private ImpersonationService _impersonationService { get; set; } = default!;
        [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
        private UserVM _currentUser = new();

        // Impersonation state
        private bool showImpersonationModal = false;
        private bool isLoadingUsers = false;
        private bool isStartingImpersonation = false;
        private string impersonationSearchTerm = "";
        private string impersonationReason = "";
        private List<UserProfileDto>? availableUsers;
        private UserProfileDto? selectedUserForImpersonation;
        private Timer? impersonationSearchTimer;

        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthProvider.GetAuthenticationStateAsync();

            if (!authState.User.Identity?.IsAuthenticated ?? true)
            {
                Navigation.NavigateTo("/");
                return;
            }

            if (!authState.User.IsInRole("Admin"))
            {
                Console.WriteLine("[Admin] User does not have Admin role, redirecting to home");
                Navigation.NavigateTo("/");
                return;
            }

            Console.WriteLine("[Admin] User has Admin role, allowing access");

            await GetCurrentUser();
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

        // Impersonation methods
        private async Task ShowImpersonationModal()
        {
            showImpersonationModal = true;
            impersonationSearchTerm = "";
            impersonationReason = "";
            selectedUserForImpersonation = null;
            availableUsers = null;

            StateHasChanged();
            await LoadUsersForImpersonation();
        }

        private void CloseImpersonationModal()
        {
            showImpersonationModal = false;
            impersonationSearchTerm = "";
            impersonationReason = "";
            selectedUserForImpersonation = null;
            availableUsers = null;
            impersonationSearchTimer?.Dispose();
            StateHasChanged();
        }

        private async Task LoadUsersForImpersonation()
        {
            isLoadingUsers = true;
            StateHasChanged();

            try
            {
                Console.WriteLine($"[Admin] Loading users for impersonation - Search: '{impersonationSearchTerm}'");

                var queryParams = new Dictionary<string, string>
                {
                    ["pageSize"] = "50", // Limit pentru performanță
                    ["page"] = "1"
                };

                if (!string.IsNullOrWhiteSpace(impersonationSearchTerm))
                {
                    queryParams["search"] = impersonationSearchTerm.Trim();
                }

                var queryString = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
                var response = await Http.GetAsync($"api/admin/users/for-impersonation?{queryString}");

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<UserProfileDto>>>();
                    if (apiResponse?.Success == true)
                    {
                        availableUsers = apiResponse.Result ?? new List<UserProfileDto>();
                        Console.WriteLine($"[Admin] Loaded {availableUsers.Count} users for impersonation");
                    }
                    else
                    {
                        Console.WriteLine($"[Admin] Failed to load users: {apiResponse?.Message}");
                        availableUsers = new List<UserProfileDto>();
                    }
                }
                else
                {
                    Console.WriteLine($"[Admin] HTTP error loading users: {response.StatusCode}");
                    availableUsers = new List<UserProfileDto>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Admin] Exception loading users: {ex.Message}");
                availableUsers = new List<UserProfileDto>();
            }
            finally
            {
                isLoadingUsers = false;
                StateHasChanged();
            }
        }

        private void OnImpersonationSearchInput(ChangeEventArgs e)
        {
            var newSearchTerm = e.Value?.ToString() ?? "";

            // Debounce search
            impersonationSearchTimer?.Dispose();
            impersonationSearchTimer = new Timer(async _ =>
            {
                await InvokeAsync(async () =>
                {
                    impersonationSearchTerm = newSearchTerm;
                    selectedUserForImpersonation = null; // Clear selection on new search
                    await LoadUsersForImpersonation();
                });
            }, null, 300, Timeout.Infinite); // 300ms debounce
        }

        private async Task ClearImpersonationSearch()
        {
            impersonationSearchTerm = "";
            selectedUserForImpersonation = null;
            await LoadUsersForImpersonation();
        }

        private void SelectUserForImpersonation(UserProfileDto user)
        {
            selectedUserForImpersonation = user;
            impersonationReason = ""; // Reset reason
            StateHasChanged();
        }

        private void CancelUserSelection()
        {
            selectedUserForImpersonation = null;
            impersonationReason = "";
            StateHasChanged();
        }

        private async Task StartImpersonation()
        {
            if (selectedUserForImpersonation == null || isStartingImpersonation)
                return;

            isStartingImpersonation = true;
            StateHasChanged();

            try
            {
                Console.WriteLine($"[Admin] Starting impersonation for user: {selectedUserForImpersonation.Name}");

                var result = await _impersonationService.StartImpersonationAsync(
                    selectedUserForImpersonation.Id,
                    impersonationReason?.Trim() ?? ""
                );

                if (result.Success)
                {
                    await JSRuntime.InvokeVoidAsync("alert", $"Successfully started impersonating {selectedUserForImpersonation.Name}. You will be redirected to their dashboard.");

                    // Redirect based on user role
                    var primaryRole = selectedUserForImpersonation.Roles?.FirstOrDefault();
                    var redirectUrl = primaryRole?.ToLower() switch
                    {
                        "moderator" => "/moderator",
                        "basic" => "/basic-user",
                        _ => "/basic-user" // Default to basic user if role is unclear
                    };

                    Navigation.NavigateTo(redirectUrl, forceLoad: true);
                }
                else
                {
                    await JSRuntime.InvokeVoidAsync("alert", $"Failed to start impersonation: {result.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Admin] Error starting impersonation: {ex.Message}");
                await JSRuntime.InvokeVoidAsync("alert", "An error occurred while starting impersonation. Please try again.");
            }
            finally
            {
                isStartingImpersonation = false;
                StateHasChanged();
            }
        }

        public void Dispose()
        {
            impersonationSearchTimer?.Dispose();
        }
    }
}
