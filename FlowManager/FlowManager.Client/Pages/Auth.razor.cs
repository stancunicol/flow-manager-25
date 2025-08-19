using FlowManager.Client.Services;
using FlowManager.Infrastructure.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using FlowManager.Shared.DTOs.Requests.Auth;

namespace FlowManager.Client.Pages
{
    public partial class Auth : ComponentBase
    {
        protected string email = string.Empty;
        protected string password = string.Empty;
        protected string? errorMessage;
        
        private bool animateLeft = false;
        private bool animateRight = false;

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
            if (authState.User.Identity?.IsAuthenticated == true)
            {
                await InvokeAsync(() => Navigation.NavigateTo("/"));
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
                var responseContent = await response.Content.ReadAsStringAsync();

                // Notify the authentication state provider first
                (CookieAuthStateProvider as CookieAuthStateProvider)?.NotifyUserAuthentication();

                // Wait for the state to be processed
                await Task.Delay(100);

                StateHasChanged();

                // Additional delay to ensure authentication state is fully processed
                await Task.Delay(200);

                var loginResponse = JsonSerializer.Deserialize<LoginResponseDto>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                switch (loginResponse?.Role?.ToUpperInvariant())
                {
                    case "ADMIN":
                        Console.WriteLine("[Auth] Redirecting to admin");
                        Navigation.NavigateTo("/admin", true); // Force reload
                        break;
                    case "BASIC":
                        Console.WriteLine("[Auth] Redirecting to basic-user");
                        Navigation.NavigateTo("/basic-user", true); // Force reload
                        break;
                    default:
                        Console.WriteLine($"[Auth] Unknown role '{loginResponse?.Role}', redirecting to home");
                        Navigation.NavigateTo("/", true); // Force reload
                        break;
                }
            }
            else
            {
                Console.WriteLine($"[Auth] Login failed with status: {response.StatusCode}");
                errorMessage = "Login failed. Please check credentials.";
            }
        }
    }
}
