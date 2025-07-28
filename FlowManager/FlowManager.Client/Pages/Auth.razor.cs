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

        protected async Task HandleLogin()
        {
            var loginData = new { Email = email, Password = password };
            var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/login?useCookies=true&useSessionCookies=true")
            {
                Content = JsonContent.Create(loginData)
            };
            request.Headers.Add("Accept", "application/json");

            var response = await Http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            if (response.IsSuccessStatusCode)
            {
                //(AuthProvider as ApiAuthenticationStateProvider)?.NotifyUserAuthentication(email);
                //(AuthProvider as ApiAuthenticationStateProvider)?.NotifyUserAuthentication();
                (CookieAuthStateProvider as CookieAuthStateProvider)?.NotifyUserAuthentication(email);
                Navigation.NavigateTo("/");
            }
            else
            {
                errorMessage = "Login failed. Please check credentials.";
            }
        }
    }
}
