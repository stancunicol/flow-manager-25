using FlowManager.Domain.Entities;
using FlowManager.Infrastructure.Context;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Infrastructure.Seed
{
    public static class UsersAndTeamsSeed
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

            var userData = new[]
            {
            new { UserName = "jameswilson", Name = "James Wilson", Email = "james.wilson@company.com", IsAdmin = false, IsModerator = false },
            new { UserName = "sarahconnor", Name = "Sarah Connor", Email = "sarah.connor@company.com", IsAdmin = false, IsModerator = false },
            new { UserName = "michaeltorres", Name = "Michael Torres", Email = "michael.torres@company.com", IsAdmin = false, IsModerator = false },
            new { UserName = "emilyrodriguez", Name = "Emily Rodriguez", Email = "emily.rodriguez@company.com", IsAdmin = false, IsModerator = false },
            new { UserName = "davidkim", Name = "David Kim", Email = "david.kim@company.com", IsAdmin = false, IsModerator = false },
            new { UserName = "jessicamartinez", Name = "Jessica Martinez", Email = "jessica.martinez@company.com", IsAdmin = false, IsModerator = false },
            new { UserName = "robertchen", Name = "Robert Chen", Email = "robert.chen@company.com", IsAdmin = false, IsModerator = false },
            new { UserName = "amandathompson", Name = "Amanda Thompson", Email = "amanda.thompson@company.com", IsAdmin = false, IsModerator = false },
            new { UserName = "christopherlee", Name = "Christopher Lee", Email = "christopher.lee@company.com", IsAdmin = false, IsModerator = false },
            new { UserName = "nicoleanderson", Name = "Nicole Anderson", Email = "nicole.anderson@company.com", IsAdmin = false, IsModerator = false },
            new { UserName = "kevinmurphy", Name = "Kevin Murphy", Email = "kevin.murphy@company.com", IsAdmin = false, IsModerator = true },
            new { UserName = "laurenfoster", Name = "Lauren Foster", Email = "lauren.foster@company.com", IsAdmin = false, IsModerator = true },
            new { UserName = "danielwhite", Name = "Daniel White", Email = "daniel.white@company.com", IsAdmin = false, IsModerator = true },
            new { UserName = "melissagarcia", Name = "Melissa Garcia", Email = "melissa.garcia@company.com", IsAdmin = true, IsModerator = false },
            new { UserName = "andrewtaylor", Name = "Andrew Taylor", Email = "andrew.taylor@company.com", IsAdmin = true, IsModerator = false }
        };

            var createdUsers = new List<User>();

            foreach (var userInfo in userData)
            {
                User? existingUser = dbContext.Users.FirstOrDefault(u => u.NormalizedUserName == userInfo.UserName.ToUpper());
                if (existingUser == null)
                {
                    var user = new User
                    {
                        Id = Guid.NewGuid(),
                        UserName = userInfo.UserName,
                        NormalizedUserName = userInfo.UserName.ToUpper(),
                        Email = userInfo.Email,
                        NormalizedEmail = userInfo.Email.ToUpper(),
                        EmailConfirmed = true,
                        Name = userInfo.Name,
                        SecurityStamp = Guid.NewGuid().ToString(),
                        ConcurrencyStamp = Guid.NewGuid().ToString()
                    };

                    user.PasswordHash = passwordHasher.HashPassword(user, "password123");
                    dbContext.Users.Add(user);
                    createdUsers.Add(user);

                    dbContext.UserRoles.Add(new UserRole
                    {
                        UserId = user.Id,
                        RoleId = basicRole.Id
                    });

                    if (userInfo.IsModerator)
                    {
                        dbContext.UserRoles.Add(new UserRole
                        {
                            UserId = user.Id,
                            RoleId = moderatorRole.Id
                        });
                    }

                    if (userInfo.IsAdmin)
                    {
                        dbContext.UserRoles.Add(new UserRole
                        {
                            UserId = user.Id,
                            RoleId = adminRole.Id
                        });
                    }
                }
            }

            dbContext.SaveChanges();

            var teamData = new[]
            {
                new { Name = "T1", UserCount = 4 },
                new { Name = "T2", UserCount = 4 },
                new { Name = "T3", UserCount = 4 },
                new { Name = "T4", UserCount = 3 }
            };

            var createdTeams = new List<Team>();
            foreach (var teamInfo in teamData)
            {
                Team? existingTeam = dbContext.Teams.FirstOrDefault(t => t.Name == teamInfo.Name);
                if (existingTeam == null)
                {
                    var team = new Team
                    {
                        Id = Guid.NewGuid(),
                        Name = teamInfo.Name
                    };
                    dbContext.Teams.Add(team);
                    createdTeams.Add(team);
                }
            }

            dbContext.SaveChanges();

            int userIndex = 0;
            foreach (var team in createdTeams)
            {
                var teamUserCount = teamData[createdTeams.IndexOf(team)].UserCount;
                for (int i = 0; i < teamUserCount && userIndex < createdUsers.Count; i++)
                {
                    var userTeam = new UserTeam
                    {
                        UserId = createdUsers[userIndex].Id,
                        TeamId = team.Id
                    };
                    dbContext.UserTeams.Add(userTeam);
                    userIndex++;
                }
            }

            dbContext.SaveChanges();
        }
    }
}
