using Microsoft.AspNetCore.Components;
using FlowManager.Shared.DTOs;
using System.Net.Http.Json;

namespace FlowManager.Client.Components.BasicUser
{
    public partial class UserDetails : ComponentBase
    {
        [Inject]
        protected HttpClient Http { get; set; } = default!;

        private UserProfileDto? userProfile;
        private bool isLoading = true;
        private string? errorMessage;

        protected override async Task OnInitializedAsync()
        {
            await LoadUserDetails();
        }

        private async Task LoadUserDetails()
        {
            try
            {
                isLoading = true;
                errorMessage = null;
                StateHasChanged();

                var response = await Http.GetAsync("api/auth/me");

                if (response.IsSuccessStatusCode)
                {
                    userProfile = await response.Content.ReadFromJsonAsync<UserProfileDto>();
                }
                else
                {
                    errorMessage = "Failed to load user details.";
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Error loading user details: {ex.Message}";
                Console.WriteLine($"[UserDetails] Error: {ex.Message}");
            }
            finally
            {
                isLoading = false;
                StateHasChanged();
            }
        }

        private async Task RefreshData()
        {
            await LoadUserDetails();
        }
    }
}