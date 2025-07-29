using FlowManager.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FlowManager.Infrastructure
{
    public class AppDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid,
        IdentityUserClaim<Guid>, UserRole, IdentityUserLogin<Guid>,
        IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Flow> Flows => Set<Flow>();
        public DbSet<Form> Forms => Set<Form>();
        public DbSet<Step> Steps => Set<Step>();
        public DbSet<StepUser> StepUsers => Set<StepUser>();
        public DbSet<StepUpdateHistory> StepUpdateHistories => Set<StepUpdateHistory>();
        public new DbSet<UserRole> UserRoles => Set<UserRole>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            
            builder.Entity<StepUser>()
                .HasKey(su => new { su.StepId, su.UserId });

            builder.Entity<StepUser>()
                .HasOne(su => su.Step)
                .WithMany(s => s.StepUsers)
                .HasForeignKey(su => su.StepId);

            builder.Entity<StepUser>()
                .HasOne(su => su.User)
                .WithMany(u => u.StepUsers)
                .HasForeignKey(su => su.UserId);

            builder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            builder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            builder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany()
                .HasForeignKey(ur => ur.RoleId);
        }
    }
}
