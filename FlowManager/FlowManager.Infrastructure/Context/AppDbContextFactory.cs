using FlowManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Infrastructure.Context
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var current = Directory.GetCurrentDirectory();

            var candidateBases = new[]
            {
                current,
                Path.GetFullPath(Path.Combine(current, "..", "FlowManager.API"))
            };

            string basePath = candidateBases.FirstOrDefault(path =>
                File.Exists(Path.Combine(path, "appsettings.Development.json")) ||
                File.Exists(Path.Combine(path, "appsettings.json")))
                ?? current;
            Console.WriteLine($"[AppDbContextFactory] Resolved basePath: {basePath}");

            var builder = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")}.json", optional: true);

            var configuration = builder.Build();

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            optionsBuilder.UseSqlite(connectionString);

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
