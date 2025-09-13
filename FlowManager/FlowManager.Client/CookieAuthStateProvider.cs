using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;
using System.Security.Claims;
using FlowManager.Domain.Entities;
using FlowManager.Shared.DTOs;

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
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
            var response = await _httpClient.SendAsync(request);

            if (response.Headers.Contains("Set-Cookie"))
            {
                Console.WriteLine($"[AuthState] Set-Cookie headers: {string.Join(", ", response.Headers.GetValues("Set-Cookie"))}");
            }

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[AuthState] Authentication failed with status: {response.StatusCode}");
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            UserProfileDto? loggedUser = await response.Content.ReadFromJsonAsync<UserProfileDto>();

            if (loggedUser != null)
            {
                var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, loggedUser.Id.ToString()),
                new Claim(ClaimTypes.Name, loggedUser.Name),
                new Claim(ClaimTypes.Email, loggedUser.Email)
            };

                if (!string.IsNullOrEmpty(loggedUser.UserName))
                {
                    claims.Add(new Claim("username", loggedUser.UserName));
                }

                foreach (var role in loggedUser.Roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                var claimsIdentity = new ClaimsIdentity(claims, "custom");
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                Console.WriteLine($"[AuthState] User authenticated: {loggedUser.Name} with roles: {string.Join(", ", loggedUser.Roles)}");

                return new AuthenticationState(claimsPrincipal);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AuthState] Error in GetAuthenticationStateAsync: {ex.Message}");
        }

        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }

    //public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    //{
    //    try
    //    {
    //        Console.WriteLine("[AuthState] Attempting to get authentication state...");

    //        var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");

    //        var response = await _httpClient.SendAsync(request);

    //        Console.WriteLine($"[AuthState] Response status: {response.StatusCode}");
    //        Console.WriteLine($"[AuthState] Response headers: {string.Join(", ", response.Headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}"))}");

    //        if (response.Headers.Contains("Set-Cookie"))
    //        {
    //            Console.WriteLine($"[AuthState] Set-Cookie headers: {string.Join(", ", response.Headers.GetValues("Set-Cookie"))}");
    //        }

    //        if (!response.IsSuccessStatusCode)
    //        {
    //            Console.WriteLine($"[AuthState] Authentication failed with status: {response.StatusCode}");
    //            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    //        }

    //        var userDto = await response.Content.ReadFromJsonAsync<UserProfileDto>();

    //        if (userDto == null || string.IsNullOrWhiteSpace(userDto.Email))
    //        {
    //            Console.WriteLine("[AuthState] User data is null or invalid");
    //            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    //        }

    //        Console.WriteLine($"[AuthState] Successfully authenticated user: {userDto.Email}");
    //        Console.WriteLine($"[AuthState] User roles: {string.Join(", ", userDto.UserRoles)}");

    //        //var claims = new List<Claim>
    //        //{
    //        //    new Claim(ClaimTypes.Name, string.IsNullOrWhiteSpace(userDto.Name) ? userDto.Email : userDto.Name),
    //        //    new Claim(ClaimTypes.Email, userDto.Email),
    //        //    new Claim(ClaimTypes.NameIdentifier, userDto.UserName ?? userDto.Email)
    //        //};

    //        //// Add role claims
    //        //foreach (var role in userDto.UserRoles)
    //        //{
    //        //    claims.Add(new Claim(ClaimTypes.Role, role));
    //        //}

    //        //var identity = new ClaimsIdentity(claims, "Cookies");

    //        //var principal = new ClaimsPrincipal(identity);
    //        //return new AuthenticationState(principal);
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine($"[AuthState] Error in GetAuthenticationStateAsync: {ex.Message}");
    //        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    //    }
    //}

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