using FlowManager.Client.Services;

namespace FlowManager.Client
{
    public static class DIConfig
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<FlowService>();
            services.AddScoped<StepService>();
            services.AddScoped<UserService>();
            services.AddScoped<ClientAuthService>();
            services.AddScoped<RoleService>();
            services.AddScoped<FormTemplateService>();
            services.AddScoped<FormResponseService>();
            services.AddScoped<ComponentService>();
            services.AddScoped<TeamService>();
            services.AddScoped<AuthService>();
            services.AddScoped<StepHistoryService>();
            services.AddScoped < ImpersonationService>();
            return services;
        }
    }
}
