using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace FlowManager.Client.Pages
{
    public partial class Auth : ComponentBase
    {
        protected string email = string.Empty;
        protected string password = string.Empty;
        protected string? errorMessage;

        [Inject] 
        protected HttpClient Http { get; set; }

        [Inject] 
        protected NavigationManager Navigation { get; set; }

        [Inject] 
        protected AuthenticationStateProvider AuthProvider { get; set; }
        
        [Inject]
        protected CookieAuthStateProvider CookieAuthStateProvider { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthProvider.GetAuthenticationStateAsync();
            if (authState.User.Identity is { IsAuthenticated: true })
            {
                Navigation.NavigateTo("/");
            }
        }

        protected async Task HandleLogin()
        {
            var loginData = new { email = email, password = password };
            
            var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/login")
            {
                Content = JsonContent.Create(loginData)
            };
            
            var response = await Http.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("[Auth] Login successful, notifying authentication state");
                (CookieAuthStateProvider as CookieAuthStateProvider)?.NotifyUserAuthentication();
                Navigation.NavigateTo("/");
            }
            else
            {
                Console.WriteLine($"[Auth] Login failed with status: {response.StatusCode}");
                errorMessage = "Login failed. Please check credentials.";
            }
        }
    }
}
