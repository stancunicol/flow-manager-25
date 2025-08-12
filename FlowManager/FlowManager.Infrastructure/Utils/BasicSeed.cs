using FlowManager.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Infrastructure.Utils
{
    public static class BasicSeed
    {
        public static void Populate(AppDbContext dbContext, IPasswordHasher<User> passwordHasher)
        {
            Role? basicRole = dbContext.Roles.FirstOrDefault(r => r.NormalizedName == "BASIC");
            if (basicRole == null)
            {
                basicRole = new Role
                {
                    Id = Guid.NewGuid(),
                    Name = "Basic",
                    NormalizedName = "BASIC",
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                };
                dbContext.Roles.Add(basicRole);
            }

            Role? moderatorRole = dbContext.Roles.FirstOrDefault(r => r.NormalizedName == "MODERATOR");
            if (moderatorRole == null)
            {
                moderatorRole = new Role
                {
                    Id = Guid.NewGuid(),
                    Name = "Moderator",
                    NormalizedName = "MODERATOR",
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                };
                dbContext.Roles.Add(moderatorRole);
            }

            Role? adminRole = dbContext.Roles.FirstOrDefault(r => r.NormalizedName == "ADMIN");
            if (adminRole == null)
            {
                adminRole = new Role
                {
                    Id = Guid.NewGuid(),
                    Name = "Admin",
                    NormalizedName = "ADMIN",
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                };
                dbContext.Roles.Add(adminRole);
            }

            dbContext.SaveChanges();

            User? basicUser = dbContext.Users.FirstOrDefault(u => u.NormalizedUserName == "BASICUSER");
            if (basicUser == null)
            {
                basicUser = new User
                {
                    Id = Guid.NewGuid(),             
                    UserName = "BasicUser",        
                    NormalizedUserName = "BASICUSER",
                    Email = "basic.user@simulator.com",   
                    NormalizedEmail = "BASIC.USER@SIMULATOR.COM",   
                    EmailConfirmed = true,           
                    Name = "Basic User",                
                    SecurityStamp = Guid.NewGuid().ToString(),
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                };
                basicUser.PasswordHash = passwordHasher.HashPassword(basicUser, "basic123");

                dbContext.Users.Add(basicUser);

                dbContext.UserRoles.Add(new UserRole
                {
                    UserId = basicUser.Id,
                    RoleId = basicRole.Id
                });
            }

            User ? moderatorUser = dbContext.Users.FirstOrDefault(u => u.NormalizedUserName == "MODERATORUSER");
            if (moderatorUser == null)
            {
                moderatorUser = new User
                {
                    Id = Guid.NewGuid(),
                    UserName = "ModeratorUser",
                    NormalizedUserName = "MODERATORUSER",
                    Email = "moderator.user@simulator.com",
                    NormalizedEmail = "MODERATOR.USER@SIMULATOR.COM",
                    EmailConfirmed = true,
                    Name = "Moderator User",
                    SecurityStamp = Guid.NewGuid().ToString(),
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                };
                moderatorUser.PasswordHash = passwordHasher.HashPassword(moderatorUser,"moderator123");

                dbContext.Users.Add(moderatorUser);

                dbContext.UserRoles.Add(new UserRole
                {
                    UserId = moderatorUser.Id,
                    RoleId = moderatorRole.Id
                });
            }

            User? adminUser = dbContext.Users.FirstOrDefault(u => u.NormalizedUserName == "ADMINUSER");
            if (adminUser == null)
            {
                adminUser = new User
                {
                    Id = Guid.NewGuid(),
                    UserName = "AdminUser",
                    NormalizedUserName = "ADMINUSER",
                    Email = "admin.user@simulator.com",
                    NormalizedEmail = "ADMIN.USER@SIMULATOR.COM",
                    EmailConfirmed = true,
                    Name = "Basic User",
                    SecurityStamp = Guid.NewGuid().ToString(),
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                };
                adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, "admin123");

                dbContext.Users.Add(adminUser);

                dbContext.UserRoles.Add(new UserRole
                {
                    UserId = adminUser.Id,
                    RoleId = adminRole.Id
                });
            }

            dbContext.SaveChanges();
        }
    }
}
