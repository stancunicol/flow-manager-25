using FlowManager.Client.Services;
using FlowManager.Infrastructure.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using FlowManager.Shared.DTOs.Requests.Auth;
using System.Diagnostics;
using System.Reflection;

namespace FlowManager.Client.Pages
{
    public partial class Auth : ComponentBase
    {
        protected string email = string.Empty;
        protected string password = string.Empty;
        protected string? errorMessage;
        protected string? version;
        protected string? simpleVersion;

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

            version = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

            simpleVersion = version?.Split(new char[] { '+', '-' })[0];
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

                (CookieAuthStateProvider as CookieAuthStateProvider)?.NotifyUserAuthentication();

                await Task.Delay(100);

                StateHasChanged();

                await Task.Delay(200);

                var loginResponse = JsonSerializer.Deserialize<LoginResponseDto>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if(loginResponse.Roles.Contains("Basic") && loginResponse.Roles.Count() == 1)
                {
                    Navigation.NavigateTo("/basic-user", true);
                }
                else
                {
                    Navigation.NavigateTo("/home", true);
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
