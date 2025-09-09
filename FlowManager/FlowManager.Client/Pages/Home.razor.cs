using FlowManager.Client.Services;
using FlowManager.Client.ViewModels;
using FlowManager.Shared.DTOs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;

namespace FlowManager.Client.Pages
{
    public partial class Home : ComponentBase
    {
        [Inject] protected HttpClient Http { get; set; } = default!;
        [Inject] protected AuthenticationStateProvider AuthProvider { get; set; } = default!;
        [Inject] private ImpersonationService _impersonationService { get; set; } = default!;

        private List<string> userRoles = new();
        private bool isLoading = true;
        private string? errorMessage;
        [Inject] private AuthService _authService { get; set; } = default!;
        private UserVM _currentUser = new();
        private bool isImpersonating = false;
        private string? originalAdminName;

        protected override async Task OnInitializedAsync()
        {
            await CheckImpersonationStatus();
            await LoadUserRoles();
            await GetCurrentUser();
        }

        private async Task CheckImpersonationStatus()
        {
            isImpersonating = await _impersonationService.IsImpersonating();
            if (isImpersonating)
            {
                originalAdminName = await _impersonationService.GetOriginalAdminName();
            }
        }

        private async Task LoadUserRoles()
        {
            try
            {
                isLoading = true;
                StateHasChanged();

                // Încarcă rolurile utilizatorului din API
                var response = await Http.GetAsync("api/auth/me");

                if (response.IsSuccessStatusCode)
                {
                    var userProfile = await response.Content.ReadFromJsonAsync<UserProfileDto>();
                    userRoles = userProfile?.Roles ?? new List<string>();

                    Console.WriteLine($"[Home] User roles loaded: {string.Join(", ", userRoles)}");
                }
                else
                {
                    Console.WriteLine($"[Home] Failed to load user data: {response.StatusCode}");
                    errorMessage = "Failed to load user data";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Home] Error loading user roles: {ex.Message}");
                errorMessage = "Error loading user data";
            }
            finally
            {
                isLoading = false;
                StateHasChanged();
            }
        }

        private void NavigateToAdmin()
        {
            Navigation.NavigateTo("/admin");
        }

        private void NavigateToUser()
        {
            Navigation.NavigateTo("/basic-user");
        }

        private void NavigateToManager()
        {
            Navigation.NavigateTo("/moderator");
        }

        private void NavigateToModerator()
        {
            Navigation.NavigateTo("/moderator");
        }

        private bool HasRole(string roleName)
        {
            return userRoles.Any(r => r.Equals(roleName, StringComparison.OrdinalIgnoreCase));
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
    }
}