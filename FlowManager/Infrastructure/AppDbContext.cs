using Microsoft.EntityFrameworkCore;
using FlowManager.Domain.Entities;

namespace FlowManager.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Flow> Flows { get; set; }
        public DbSet<Form> Forms { get; set; }
        public DbSet<Step> Steps { get; set; }
    }
}
