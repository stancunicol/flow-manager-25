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
        public DbSet<FlowStepItemUser> FlowStepItemUsers => Set<FlowStepItemUser>();
        public DbSet<FlowStepItemTeam> FlowStepItemTeams => Set<FlowStepItemTeam>();
        public DbSet<UserTeam> UserTeams => Set<UserTeam>();
        public DbSet<FormTemplateFlow> FormTemplateFlows => Set<FormTemplateFlow>();
        public DbSet<FormReview> FormReviews => Set<FormReview>();
        public DbSet<StepHistory> StepHistory => Set<StepHistory>();
        public DbSet<FlowStepItem> FlowStepItems => Set<FlowStepItem>();    

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            UniqueUserEmailConstraintConfiguration(builder);
            UniqueRolenameConstraintConfiguration(builder);
            UniqueFormTemplateConstraintConfiguration(builder);
            UniqueTeamNameConstraintConfiguration(builder);

            UserRoleRelationshipConfiguration(builder);
            FormTemplateFlowsRelationshipConfiguration(builder);
            UserTeamRelationshipConfiguration(builder);
            CompleteOnBehalfUserFormResponseConfiguration(builder);

            FlowStepItemTeamConfiguration(builder);
            FlowStepItemUserConfiguration(builder);

            FormReviewRelationshipConfiguration(builder);

            JSONConfiguration(builder);

            builder.Entity<StepHistory>(entity =>
            {
                entity.HasKey(e => e.IdStepHistory);
                entity.Property(e => e.Action).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Details).HasMaxLength(100);
                entity.Property(e => e.DateTime).IsRequired();

                entity.HasOne(e => e.Step)
                    .WithMany()
                    .HasForeignKey(e => e.StepId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private void FlowStepItemTeamConfiguration(ModelBuilder builder)
        {
            builder.Entity<FlowStepItemTeam>(entity =>
            {
                entity.HasKey(e => new { e.FlowStepItemId, e.TeamId });

                entity.HasOne(e => e.FlowStepItem)
                    .WithMany(fsi => fsi.AssignedTeams)
                    .HasForeignKey(e => e.FlowStepItemId)
                    .IsRequired();

                entity.HasOne(e => e.Team)
                    .WithMany(t => t.FlowStepTeams)
                    .HasForeignKey(e => e.TeamId)
                    .IsRequired();
            });
        }

        private void FlowStepItemUserConfiguration(ModelBuilder builder)
        {
            builder.Entity<FlowStepItemUser>(entity =>
            {
                entity.HasKey(e => new { e.FlowStepItemId, e.UserId });

                entity.HasOne(e => e.FlowStepItem)
                    .WithMany(fsi => fsi.AssignedUsers)
                    .HasForeignKey(e => e.FlowStepItemId)
                    .IsRequired();

                entity.HasOne(e => e.User)
                    .WithMany(u => u.FlowStepUsers)
                    .HasForeignKey(e => e.UserId)
                    .IsRequired();
            });
        }

        private void FormReviewRelationshipConfiguration(ModelBuilder builder)
        {
            builder.Entity<FormReview>(entity =>
            {
                entity.HasKey(fr => fr.Id);

                entity.HasOne(fr => fr.FormResponse)
                    .WithMany()
                    .HasForeignKey(fr => fr.FormResponseId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(fr => fr.Reviewer)
                    .WithMany()
                    .HasForeignKey(fr => fr.ReviewerId)
                    .HasForeignKey(fr => fr.ReviewerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(fr => fr.Step)
                    .WithMany()
                    .HasForeignKey(fr => fr.StepId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(fr => fr.Action)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(fr => fr.RejectReason)
                    .HasMaxLength(500);

                entity.HasIndex(fr => new { fr.ReviewerId, fr.ReviewedAt });
                entity.HasIndex(fr => fr.FormResponseId);
            });
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

        private void UserTeamRelationshipConfiguration(ModelBuilder builder)
        {
            builder.Entity<UserTeam>(entity =>
            {
                entity.HasKey(ut => new { ut.UserId, ut.TeamId });

                entity.HasOne(ut => ut.User)
                      .WithMany(u => u.Teams)
                      .HasForeignKey(ut => ut.UserId)
                      .IsRequired();

                entity.HasOne(ut => ut.Team)
                      .WithMany(t => t.Users)
                      .HasForeignKey(ut => ut.TeamId)
                      .IsRequired();
            });

            builder.Entity<User>(entity =>
            {
                entity.ToTable("AspNetUsers");
            });

            builder.Entity<Team>(entity =>
            {
                entity.ToTable("Teams");
            });
        }

        private void FormTemplateFlowsRelationshipConfiguration(ModelBuilder builder)
        {
            builder.Entity<FormTemplateFlow>(entity =>
            {
                entity.HasKey(ftf => new { ftf.FormTemplateId, ftf.FlowId });

                entity.HasOne(ftf => ftf.FormTemplate)
                      .WithMany(u => u.FormTemplateFlows)
                      .HasForeignKey(ut => ut.FormTemplateId)
                      .IsRequired();

                entity.HasOne(ftf => ftf.Flow)
                      .WithMany(u => u.FormTemplateFlows)
                      .HasForeignKey(ut => ut.FlowId)
                      .IsRequired();
            });

            builder.Entity<FormTemplate>(entity =>
            {
                entity.ToTable("FormTemplates");
            });

            builder.Entity<Flow>(entity =>
            {
                entity.ToTable("Flows");
            });

            builder.Entity<FormTemplate>()
                .HasIndex(ft => ft.Name)
                .IsUnique();

            builder.Entity<Flow>()
                .HasIndex(f => f.Name)
                .IsUnique();
        }

        private void JSONConfiguration(ModelBuilder builder)
        {
            builder.Entity<Component>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Properties)
                      .HasConversion(
                          v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                          v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions)null))
                      .HasColumnType("TEXT"); // JSON
            });

            builder.Entity<FormResponse>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ResponseFields)
                      .HasConversion(
                          v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                          v => JsonSerializer.Deserialize<Dictionary<Guid, object>>(v, (JsonSerializerOptions)null))
                      .HasColumnType("TEXT"); // JSON
            });
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

        private void UniqueTeamNameConstraintConfiguration(ModelBuilder builder)
        {
            builder.Entity<Team>()
                .HasIndex(t => t.Name)
                .IsUnique()
                .HasDatabaseName("IX_Teams_Name");
        }

        private void CompleteOnBehalfUserFormResponseConfiguration(ModelBuilder builder)
        {
            builder.Entity<FormResponse>()
                .HasOne(formResponse => formResponse.CompletedByOtherUser)
                .WithMany(user => user.FormResponseCompletedOnBehalf)
                .HasForeignKey(formResponse => formResponse.CompletedByOtherUserId);
        }
    }
}