using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace FlowManager.Client.Pages
{
    public partial class BasicUser : ComponentBase
    {
        private string _activeTab = "MYFORMS";
        protected string? errorMessage;

        [Inject]
        protected NavigationManager Navigation { get; set; }

        [Inject]
        protected AuthenticationStateProvider AuthProvider { get; set; }

        [Inject]
        protected CookieAuthStateProvider CookieAuthStateProvider { get; set; }

        [Inject]
        protected HttpClient Http { get; set; }

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
            // Logic for adding a new form
            Console.WriteLine("Add Form clicked");
        }
    }
}