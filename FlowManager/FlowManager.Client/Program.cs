using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using FlowManager.Client;
using Microsoft.AspNetCore.Identity;
using FlowManager.Infrastructure.Services;
using FlowManager.Application.Interfaces;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");


var app = builder.Build();


// Added services for FormService and FlowService
builder.Services.AddScoped<IFlowService, FlowService>();
builder.Services.AddScoped<IFormService, FormService>();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });


await builder.Build().RunAsync();
