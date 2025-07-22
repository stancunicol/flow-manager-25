using FlowManager.Application;
using FlowManager.Domain.Entities;
using FlowManager.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;





var builder = WebApplication.CreateBuilder(args);

//CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policyBuilder =>
    {
        policyBuilder.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
    });
});

// Add layer dependencies
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddAuthorization();
builder.Services.AddAuthentication();

builder.Services.ConfigureApplicationCookie(Options =>
{
    Options.LoginPath = "/api/auth/login";
    Options.LogoutPath = "/api/auth/logout";
    Options.AccessDeniedPath = "/api/auth/denied";
    Options.ExpireTimeSpan = TimeSpan.FromDays(30);
});



builder.Services.AddIdentityCore<User>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddApiEndpoints();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//Use CORS

app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

//app.MapIdentityApi<User>(options =>
//{
//    options.MapLogin = false;     
//    options.MapLogout = false;
//    options.MapUserInfo = true;   
//    options.MapRegister = true;   
//});

app.MapIdentityApi<User>();

app.MapControllers();

app.Run();
