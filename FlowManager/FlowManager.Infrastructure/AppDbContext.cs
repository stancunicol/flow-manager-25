using FlowManager.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using System.Text.Json;

namespace FlowManager.Infrastructure
{
    public class AppDbContext : IdentityDbContext<User, Role, Guid,
        IdentityUserClaim<Guid>, UserRole, IdentityUserLogin<Guid>,
        IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Flow> Flows => Set<Flow>();
        public DbSet<Component> Components => Set<Component>();
        public DbSet<Step> Steps => Set<Step>();
        public DbSet<FlowStep> FlowSteps => Set<FlowStep>();
        public DbSet<FormTemplate> FormTemplates => Set<FormTemplate>();    
        public DbSet<FormResponse> FormResponses => Set<FormResponse>();
        public DbSet<FormTemplateComponent> FormTemplateComponents => Set<FormTemplateComponent>();
        public DbSet<User> Users => Set<User>();    
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Component>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Properties)
                      .HasConversion(
                          v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                          v => JsonSerializer.Deserialize<Dictionary<string,object>>(v, (JsonSerializerOptions)null))
                      .HasColumnType("jsonb"); // PostgreSQL jsonb type
            });

            builder.Entity<FormTemplateComponent>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Properties)
                      .HasConversion(
                          v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                          v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions)null))
                      .HasColumnType("jsonb"); // PostgreSQL jsonb type
            });

            builder.Entity<FormResponse>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.ResponseFields)
                      .HasConversion(
                          v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                          v => JsonSerializer.Deserialize<Dictionary<Guid, object>>(v, (JsonSerializerOptions)null))
                      .HasColumnType("jsonb"); // PostgreSQL jsonb type
            });
        }
    }
}
