using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;
using System.Security.Claims;
using FlowManager.Domain.Entities;

public class CookieAuthStateProvider : AuthenticationStateProvider
{
    private readonly HttpClient _httpClient;

    public CookieAuthStateProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/auth/me");
            // if (!response.IsSuccessStatusCode)
            // {
            //     Console.WriteLine("0");
            //     return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            // }
                

            var user = await response.Content.ReadFromJsonAsync<User>();

            // if (user == null || string.IsNullOrWhiteSpace(user.Email))
            // {
            //     Console.WriteLine("1");
            //     return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            // }

            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, string.IsNullOrWhiteSpace(user.Name) ? user.Email : user.Name),
                new Claim(ClaimTypes.Email, user.Email)
            }, "Cookies");

            var principal = new ClaimsPrincipal(identity);
            Console.WriteLine("OK");
            return new AuthenticationState(principal);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AuthState] Eroare la GetAuthenticationStateAsync: {ex.Message}");
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    public void NotifyUserAuthentication()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public void NotifyUserLogout()
    {
        var anonymous = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        NotifyAuthenticationStateChanged(Task.FromResult(anonymous));
    }
}