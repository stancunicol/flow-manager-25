using FlowManager.Application.Interfaces;
using FlowManager.Application.Services;
using FlowManager.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FlowManager.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddMemoryCache();

            // Application Services
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IPasswordResetService, PasswordResetService>();

            // Infrastructure Services (these are in Infrastructure namespace but should be registered here)
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IFormTemplateService, FormTemplateService>();
            services.AddScoped<IRoleService, RoleService>();

            return services;
        }
    }
}