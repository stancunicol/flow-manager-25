using FlowManager.Application.Interfaces;
using FlowManager.Application.IServices;
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

            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IPasswordResetService, PasswordResetService>();

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IFormTemplateService, FormTemplateService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IStepService, StepService>();
            services.AddScoped<ITeamService, TeamService>();
            services.AddScoped<IComponentService, ComponentService>();
            services.AddScoped<IFlowService, FlowService>();
            services.AddScoped<IFormResponseService, FormResponseService>();
            services.AddScoped<IStepHistoryService, StepHistoryService>();

            return services;
        }
    }
}