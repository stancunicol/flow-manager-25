using FlowManager.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using System.Text.Json;

namespace FlowManager.Infrastructure.Context
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
        public DbSet<Team> Teams => Set<Team>();
        public DbSet<StepUser> StepUsers => Set<StepUser>();
        public DbSet<StepTeam> StepTeams => Set<StepTeam>();
        public DbSet<FlowStepUser> FlowStepUsers => Set<FlowStepUser>();
        public DbSet<FlowStepTeam> FlowStepTeams => Set<FlowStepTeam>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            UniqueUserEmailConstraintConfiguration(builder);
            UniqueRolenameConstraintConfiguration(builder);
            UniqueFormTemplateConstraintConfiguration(builder);
            UniqueTeamNameConstraintConfiguration(builder);

            UniqueFlowStepKeyConstraintConfiguration(builder);

            StepUserKeyConstraintConfiguration(builder); 
            StepTeamKeyConstraintConfiguration(builder);

            UserRoleRelationshipConfiguration(builder);
            TeamRelationshipConfiguration(builder);

            JSONBConfiguration(builder);
        }

        private void UserRoleRelationshipConfiguration(ModelBuilder builder)
        {
            builder.Entity<User>(entity =>
            {
                entity.ToTable("AspNetUsers");


                entity.HasMany(u => u.Roles)
                      .WithOne(ur => ur.User)
                      .HasForeignKey(ur => ur.UserId)
                      .IsRequired();
            });

            builder.Entity<Role>(entity =>
            {
                entity.ToTable("AspNetRoles");

                entity.HasMany(r => r.Users)
                      .WithOne(ur => ur.Role)
                      .HasForeignKey(ur => ur.RoleId)
                      .IsRequired();
            });
        }
        private void TeamRelationshipConfiguration(ModelBuilder builder)
        {
            // User -> Team (1-to-many)
            builder.Entity<User>(entity =>
            {
                entity.HasOne(u => u.Team)
                      .WithMany(t => t.Users)
                      .HasForeignKey(u => u.TeamId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // StepUser (many-to-many Step <-> User)
            builder.Entity<StepUser>(entity =>
            {
                entity.HasOne(su => su.Step)
                      .WithMany(s => s.Users)
                      .HasForeignKey(su => su.StepId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(su => su.User)
                      .WithMany(u => u.Steps)
                      .HasForeignKey(su => su.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // StepTeam (many-to-many Step <-> Team)
            builder.Entity<StepTeam>(entity =>
            {
                entity.HasOne(st => st.Step)
                      .WithMany(s => s.Teams)
                      .HasForeignKey(st => st.StepId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(st => st.Team)
                      .WithMany(t => t.Steps)
                      .HasForeignKey(st => st.TeamId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }


        private void JSONBConfiguration(ModelBuilder builder)
        {
            builder.Entity<Component>(entity =>
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

            builder.Entity<FormTemplateComponent>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Properties)
                     .HasConversion(
                         v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                         v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions)null))
                     .HasColumnType("jsonb"); // PostgreSQL jsonb type
            });
        }

        private void UniqueFlowStepKeyConstraintConfiguration(ModelBuilder builder)
        {
            builder.Entity<FlowStep>()
                .HasKey(fs => new { fs.FlowId, fs.StepId });
        }

        private void StepUserKeyConstraintConfiguration(ModelBuilder builder)
        {
            builder.Entity<StepUser>()
                .HasIndex(su => new { su.StepId, su.UserId })
                .IsUnique()
                .HasDatabaseName("IX_StepUsers_StepId_UserId");
        }

        private void StepTeamKeyConstraintConfiguration(ModelBuilder builder)
        {
            builder.Entity<StepTeam>()
                .HasIndex(st => new { st.StepId, st.TeamId })
                .IsUnique()
                .HasDatabaseName("IX_StepTeams_StepId_TeamId");
        }

        private void UniqueRolenameConstraintConfiguration(ModelBuilder builder)
        {
            builder.Entity<Role>()
                    .HasIndex(r => r.Name)
                    .IsUnique()
                    .HasDatabaseName("IX_AspNetRoles_Name");
            builder.Entity<Role>()
                    .HasIndex(r => r.NormalizedName)
                    .IsUnique()
                    .HasDatabaseName("IX_AspNetRoles_NormalizedName");
        }

        private void UniqueUserEmailConstraintConfiguration(ModelBuilder builder)
        {
            builder.Entity<User>()
                    .HasIndex(u => u.Email)
                    .IsUnique()
                    .HasDatabaseName("IX_AspNetUsers_Email");

            builder.Entity<User>()
                    .HasIndex(u => u.NormalizedEmail)
                    .IsUnique()
                    .HasDatabaseName("IX_AspNetUsers_NormalizedEmail");
        }

        private void UniqueFormTemplateConstraintConfiguration(ModelBuilder builder)
        {
            builder.Entity<FormTemplate>()
                .HasIndex(ft => ft.Name)
                .IsUnique()
                .HasDatabaseName("IX_FormTemplates_Name");
        }
        // NOU - Configurare unicitate pentru Team name
        private void UniqueTeamNameConstraintConfiguration(ModelBuilder builder)
        {
            builder.Entity<Team>()
                .HasIndex(t => t.Name)
                .IsUnique()
                .HasDatabaseName("IX_Teams_Name");
        }
    }
}
