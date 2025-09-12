using System.Security.Claims;
using System.Net.Http.Json;
using FlowManager.Domain.Entities;
using Microsoft.AspNetCore.Components.Authorization;

public class ApiAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly HttpClient _httpClient;
    private string _email; 

    public ApiAuthenticationStateProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public void SetEmail(string email)
    {
        _email = email;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/Users/email/{_email}");
            
            if (!response.IsSuccessStatusCode)
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            var user = await response.Content.ReadFromJsonAsync<User>();
            Console.WriteLine(user.Email);


            if (user == null || string.IsNullOrEmpty(user.Email))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }


            var identity = new ClaimsIdentity(new[]
            {

                new Claim(ClaimTypes.Name, user.Name ?? ""),
                new Claim(ClaimTypes.Email, user.Email)
            }, "apiauth");




            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
        catch (Exception ex)
        {
            // Loghează detaliile excepției aici dacă ai un mecanism de logare
            Console.WriteLine($"Error retrieving authentication state: {ex.Message}");
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    public void NotifyUserAuthentication(string email)
    {
        SetEmail(email);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public void NotifyUserLogout()  
    {
        var anonymous = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        NotifyAuthenticationStateChanged(Task.FromResult(anonymous));
    }
}

