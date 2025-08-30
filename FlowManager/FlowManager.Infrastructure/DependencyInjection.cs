using FlowManager.Domain.Entities;
using FlowManager.Domain.IRepositories;
using FlowManager.Infrastructure.Context;
using FlowManager.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FlowManager.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                     options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<User, Role>()
                    .AddEntityFrameworkStores<AppDbContext>()
                    .AddDefaultTokenProviders();

            // Repositories only
            services.AddScoped<IComponentRepository, ComponentRepository>();
            services.AddScoped<IFlowRepository, FlowRepository>();
            services.AddScoped<IFormTemplateRepository, FormTemplateRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IStepRepository, StepRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IFormResponseRepository, FormResponseRepository>();
            services.AddScoped<ITeamRepository, TeamRepository>();
            services.AddScoped<IFlowStepRepository, FlowStepRepository>();

            return services;
        }
    }
}