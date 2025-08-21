using FlowManager.Shared.DTOs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;

namespace FlowManager.Client.Pages
{
    public partial class Home : ComponentBase
    {
        [Inject] protected NavigationManager Navigation { get; set; } = default!;
        [Inject] protected HttpClient Http { get; set; } = default!;
        [Inject] protected AuthenticationStateProvider AuthProvider { get; set; } = default!;

        private List<string> userRoles = new();
        private bool isLoading = true;
        private string? errorMessage;

        protected override async Task OnInitializedAsync()
        {
            await LoadUserRoles();
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
            Navigation.NavigateTo("/manager-dashboard");
        }

        private void NavigateToModerator()
        {
            // Redirecționează moderatorii către admin sau creați o pagină separată
            Navigation.NavigateTo("/admin"); // sau "/moderator-dashboard"
        }

        private bool HasRole(string roleName)
        {
            return userRoles.Any(r => r.Equals(roleName, StringComparison.OrdinalIgnoreCase));
        }
    }
}