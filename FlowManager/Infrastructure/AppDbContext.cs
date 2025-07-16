using Microsoft.EntityFrameworkCore;
using FlowManager.Domain.Entities;

namespace FlowManager.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Flow> Flows { get; set; }
        public DbSet<Form> Forms { get; set; }
        public DbSet<Step> Steps { get; set; }
        public DbSet<StepUpdateHistory> StepUpdateHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Flow -> Steps (one-to-many)
            modelBuilder.Entity<Step>()
                .HasOne(s => s.Flow)
                .WithMany(f => f.Steps)
                .HasForeignKey(s => s.FlowId)
                .OnDelete(DeleteBehavior.Cascade);

            // Flow -> Forms (one-to-many)
            modelBuilder.Entity<Form>()
                .HasOne(f => f.Flow)
                .WithMany(fl => fl.Forms)
                .HasForeignKey(f => f.FlowId)
                .OnDelete(DeleteBehavior.Cascade);

            // User -> Forms (one-to-many)
            modelBuilder.Entity<Form>()
                .HasOne(f => f.User)
                .WithMany(u => u.Forms)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Step -> Forms (one-to-many) for LastStep
            modelBuilder.Entity<Form>()
                .HasOne(f => f.LastStep)
                .WithMany()
                .HasForeignKey(f => f.LastStepId)
                .OnDelete(DeleteBehavior.Restrict);

            // User <-> Step (many-to-many) for AssignedUsers
            modelBuilder.Entity<User>()
                .HasMany(u => u.AssignedSteps)
                .WithMany(s => s.AssignedUsers)
                .UsingEntity(j => j.ToTable("UserStepAssignments"));

            // StepUpdateHistory relationships
            modelBuilder.Entity<StepUpdateHistory>()
                .HasOne(suh => suh.Step)
                .WithMany(s => s.UpdateHistories)
                .HasForeignKey(suh => suh.StepId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StepUpdateHistory>()
                .HasOne(suh => suh.User)
                .WithMany(u => u.UpdateHistories)
                .HasForeignKey(suh => suh.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // User -> UserRoles (one-to-many)
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes for better query performance
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Form>()
                .HasIndex(f => f.FlowId);

            modelBuilder.Entity<Form>()
                .HasIndex(f => f.UserId);

            modelBuilder.Entity<Form>()
                .HasIndex(f => f.LastStepId);

            modelBuilder.Entity<Form>()
                .HasIndex(f => f.Status);

            modelBuilder.Entity<Step>()
                .HasIndex(s => s.FlowId);

            modelBuilder.Entity<UserRole>()
                .HasIndex(ur => ur.UserId);

            modelBuilder.Entity<UserRole>()
                .HasIndex(ur => new { ur.UserId, ur.Role })
                .IsUnique();

            modelBuilder.Entity<StepUpdateHistory>()
                .HasIndex(suh => suh.StepId);

            modelBuilder.Entity<StepUpdateHistory>()
                .HasIndex(suh => suh.UserId);

            modelBuilder.Entity<StepUpdateHistory>()
                .HasIndex(suh => suh.UpdatedAt);
        }
    }
}
