using FlowManager.Application.Interfaces;
using FlowManager.Domain.Entities;
using FlowManager.Domain.IRepositories;
using FlowManager.Infrastructure.Context;
using FlowManager.Infrastructure.Repositories;
using FlowManager.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL;




namespace FlowManager.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
                 options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddIdentity<User, IdentityRole<Guid>>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

        services.AddScoped<IPasswordResetService, PasswordResetService>();

        services.AddMemoryCache();

        services.AddScoped<IComponentRepository, ComponentRepository>();
        services.AddScoped<IFlowRepository, FlowRepository>();
        // services.AddScoped<IFormRepository, FormRepository>();
        services.AddScoped<IFormTemplateRepository, FormTemplateRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IStepRepository, StepRepository>();
        services.AddScoped<IUserRepository, UserRepository>();


        return services;
    }
}