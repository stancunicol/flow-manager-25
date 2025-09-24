using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using FlowManager.Client;
using FlowManager.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<CookieAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider =>
    provider.GetRequiredService<CookieAuthStateProvider>());

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddApiAuthorization();

builder.Services.AddScoped(sp => 
{
    var httpClient = new HttpClient()
    {
        BaseAddress = new Uri("http://localhost:5000/")
    };
    
    httpClient.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
    
    return httpClient;
});

builder.Services.AddScoped<FlowService>();

builder.Services.AddServices();

builder.Services.AddScoped<FormReviewService>();

builder.Services.AddBlazorBootstrap();

await builder.Build().RunAsync();