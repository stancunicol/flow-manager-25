using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

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

        return services;
    }
}