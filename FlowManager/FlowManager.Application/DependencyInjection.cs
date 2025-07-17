using Microsoft.Extensions.DependencyInjection;
using FluentValidation;

namespace FlowManager.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Add AutoMapper
        services.AddAutoMapper(typeof(DependencyInjection).Assembly);

        // Add MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        // Add FluentValidation
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        // Add Application Services
        // services.AddScoped<IUserService, UserService>();
        // services.AddScoped<IFlowService, FlowService>();

        return services;
    }
}