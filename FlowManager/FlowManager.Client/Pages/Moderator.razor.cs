using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace FlowManager.Client.Pages
{
    public partial class Moderator : ComponentBase
    {
        private string _activeTab = "ASSIGNED";
        protected string? errorMessage;

        // Search and loading state
        private string searchTerm = "";
        private bool isLoading = false;
        private Timer? searchDebounceTimer;

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

            Console.WriteLine("[Moderator] User has Moderator role, allowing access");
            await LoadAssignedForms();
        }

        private void SetActiveTab(string tab)
        {
            _activeTab = tab;
        }

        private async Task LoadAssignedForms()
        {
            isLoading = true;
            StateHasChanged();

            try
            {
                // TODO: Implementează încărcarea formularelor atribuite
                await Task.Delay(1000); // Simulează loading pentru moment
                Console.WriteLine("[Moderator] Loading assigned forms...");
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

        private async Task RefreshAssignedForms()
        {
            searchTerm = "";
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
                await InvokeAsync(async () =>
                {
                    await LoadAssignedForms();
                });
            }, null, 500, Timeout.Infinite); // 500ms debounce
        }

        private async Task ClearSearch()
        {
            searchTerm = "";
            await LoadAssignedForms();
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