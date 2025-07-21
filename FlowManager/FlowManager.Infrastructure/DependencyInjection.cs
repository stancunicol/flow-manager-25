using FlowManager.Application.Interfaces;
using FlowManager.Domain.Entities;
using FlowManager.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;




namespace FlowManager.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Add Entity Framework
        // services.AddDbContext<FlowManagerDbContext>(options =>
        //     options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Add Repositories
        // services.AddScoped<IUserRepository, UserRepository>();
        // services.AddScoped<IFlowRepository, FlowRepository>();

        // Add Unit of Work
        // services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Add External Services
        // services.AddScoped<IEmailService, EmailService>();
        // services.AddScoped<IFileStorageService, FileStorageService>();

        // Add Infrastructure Services
        // services.AddScoped<IDateTime, DateTimeService>();
        // services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddIdentity<User, IdentityRole<Guid>>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IUserRoleService, UserRoleService>();
        services.AddScoped<IFlowService, FlowService>();

        return services;
    }
}