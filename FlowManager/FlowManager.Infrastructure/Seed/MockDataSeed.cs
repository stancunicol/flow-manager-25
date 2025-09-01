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
    public static class MockDataSeed
    {
        public static void Populate(AppDbContext dbContext, IPasswordHasher<User> passwordHasher)
        {
            // Create roles first
            var roles = CreateRoles(dbContext);

            // Create steps
            var steps = CreateSteps(dbContext);

            // Create users with role assignments and step assignments
            var users = CreateUsers(dbContext, passwordHasher, roles, steps);

            // Create teams based on steps
            var teams = CreateTeamsBasedOnSteps(dbContext, steps);

            // Create user-team relationships based on step assignments
            CreateUserTeamRelationships(dbContext, users, teams);

            // Create components
            var components = CreateComponents(dbContext);

            // Create form templates
            var formTemplates = CreateFormTemplates(dbContext);

            // Create sample form responses
            CreateSampleFormResponses(dbContext, users, formTemplates, steps, components);

            dbContext.SaveChanges();
        }

        private static (Role basicRole, Role moderatorRole, Role adminRole) CreateRoles(AppDbContext dbContext)
        {
            Role? basicRole = dbContext.Roles.FirstOrDefault(r => r.NormalizedName == "BASIC");
            if (basicRole == null)
            {
                basicRole = new Role
                {
                    Id = Guid.NewGuid(),
                    Name = "Basic",
                    NormalizedName = "BASIC",
                    ConcurrencyStamp = Guid.NewGuid().ToString(),
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
            return (basicRole, moderatorRole, adminRole);
        }

        private static List<User> CreateUsers(AppDbContext dbContext, IPasswordHasher<User> passwordHasher,
            (Role basicRole, Role moderatorRole, Role adminRole) roles, List<Step> steps)
        {
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
            var random = new Random();

            foreach (var userInfo in userData)
            {
                User? existingUser = dbContext.Users.FirstOrDefault(u => u.NormalizedUserName == userInfo.UserName.ToUpper());
                if (existingUser == null)
                {
                    // Assign random step to user
                    var assignedStep = steps[random.Next(steps.Count)];

                    var user = new User
                    {
                        Id = Guid.NewGuid(),
                        UserName = userInfo.UserName,
                        NormalizedUserName = userInfo.UserName.ToUpper(),
                        Email = userInfo.Email,
                        NormalizedEmail = userInfo.Email.ToUpper(),
                        EmailConfirmed = true,
                        Name = userInfo.Name,
                        StepId = assignedStep.Id, // Assign step to user
                        SecurityStamp = Guid.NewGuid().ToString(),
                        ConcurrencyStamp = Guid.NewGuid().ToString()
                    };

                    user.PasswordHash = passwordHasher.HashPassword(user, "password123");
                    dbContext.Users.Add(user);
                    createdUsers.Add(user);

                    dbContext.UserRoles.Add(new UserRole
                    {
                        UserId = user.Id,
                        RoleId = roles.basicRole.Id
                    });

                    if (userInfo.IsModerator)
                    {
                        dbContext.UserRoles.Add(new UserRole
                        {
                            UserId = user.Id,
                            RoleId = roles.moderatorRole.Id
                        });
                    }

                    if (userInfo.IsAdmin)
                    {
                        dbContext.UserRoles.Add(new UserRole
                        {
                            UserId = user.Id,
                            RoleId = roles.adminRole.Id
                        });
                    }
                }
                else
                {
                    createdUsers.Add(existingUser);
                }
            }

            dbContext.SaveChanges();
            return createdUsers;
        }

        private static List<Team> CreateTeamsBasedOnSteps(AppDbContext dbContext, List<Step> steps)
        {
            var createdTeams = new List<Team>();
            int teamCounter = 1;

            foreach (var step in steps)
            {
                // Create team name based on step
                string teamName = $"{step.Name} Team {teamCounter}";

                Team? existingTeam = dbContext.Teams.FirstOrDefault(t => t.Name == teamName);
                if (existingTeam == null)
                {
                    var team = new Team
                    {
                        Id = Guid.NewGuid(),
                        Name = teamName,
                    };
                    dbContext.Teams.Add(team);
                    createdTeams.Add(team);
                }
                else
                {
                    createdTeams.Add(existingTeam);
                }
                teamCounter++;
            }

            dbContext.SaveChanges();
            return createdTeams;
        }

        private static void CreateUserTeamRelationships(AppDbContext dbContext, List<User> users, List<Team> teams)
        {
            var random = new Random();

            // Group users by their step assignment
            var usersByStep = users.GroupBy(u => u.StepId).ToList();

            foreach (var userGroup in usersByStep)
            {
                var stepId = userGroup.Key;
                var stepUsers = userGroup.ToList();
                var step = dbContext.Steps.First(s => s.Id == stepId);

                // Find existing teams for this step
                var stepTeams = teams.Where(t => t.Name.Contains(step.Name)).ToList();

                // Create teams of 3-4 users from the same step
                var teamSize = random.Next(3, 5); // 3 or 4 users per team
                var currentTeamIndex = 0;

                for (int i = 0; i < stepUsers.Count; i++)
                {
                    var user = stepUsers[i];

                    // If we need a new team for this step
                    if (i > 0 && i % teamSize == 0)
                    {
                        currentTeamIndex++;
                        teamSize = random.Next(3, 5); // New team size
                    }

                    // Create additional team if needed
                    if (currentTeamIndex >= stepTeams.Count)
                    {
                        var newTeamName = $"{step.Name} Team {currentTeamIndex + 1}";

                        // Check if team with this name already exists in database
                        var existingTeam = dbContext.Teams.FirstOrDefault(t => t.Name == newTeamName);

                        if (existingTeam == null)
                        {
                            var newTeam = new Team
                            {
                                Id = Guid.NewGuid(),
                                Name = newTeamName
                                // Add StepId if the Team model has this property
                                // StepId = stepId
                            };
                            dbContext.Teams.Add(newTeam);
                            dbContext.SaveChanges(); // Save immediately to avoid conflicts
                            stepTeams.Add(newTeam);
                            teams.Add(newTeam);
                        }
                        else
                        {
                            stepTeams.Add(existingTeam);
                        }
                    }

                    var targetTeam = stepTeams[currentTeamIndex];

                    // Check if relationship already exists
                    bool relationshipExists = dbContext.UserTeams.Any(ut =>
                        ut.UserId == user.Id && ut.TeamId == targetTeam.Id);

                    if (!relationshipExists)
                    {
                        var userTeam = new UserTeam
                        {
                            UserId = user.Id,
                            TeamId = targetTeam.Id
                        };
                        dbContext.UserTeams.Add(userTeam);
                    }
                }
            }

            dbContext.SaveChanges();
        }

        private static List<Step> CreateSteps(AppDbContext dbContext)
        {
            var steps = new List<Step>
        {
            new Step
            {
                Id = Guid.NewGuid(),
                Name = "HR",
                CreatedAt = DateTime.UtcNow
            },
            new Step
            {
                Id = Guid.NewGuid(),
                Name = "IT",
                CreatedAt = DateTime.UtcNow
            },
            new Step
            {
                Id = Guid.NewGuid(),
                Name = "Hardware development",
                CreatedAt = DateTime.UtcNow
            },
            new Step
            {
                Id = Guid.NewGuid(),
                Name = "Finances",
                CreatedAt = DateTime.UtcNow
            },
            new Step
            {
                Id = Guid.NewGuid(),
                Name = "Software development",
                CreatedAt = DateTime.UtcNow
            }
        };

            dbContext.Steps.AddRange(steps);
            dbContext.SaveChanges();
            return steps;
        }

        private static List<Component> CreateComponents(AppDbContext dbContext)
        {
            var components = new List<Component>
        {
            new Component
            {
                Id = Guid.NewGuid(),
                Type = "text",
                Label = "Full Name",
                Required = true,
                Properties = new Dictionary<string, object>
                {
                    { "placeholder", "Enter your full name" },
                    { "maxLength", 100 }
                },
                CreatedAt = DateTime.UtcNow
            },
            new Component
            {
                Id = Guid.NewGuid(),
                Type = "email",
                Label = "Email Address",
                Required = true,
                Properties = new Dictionary<string, object>
                {
                    { "placeholder", "Enter your email address" },
                    { "validation", "email" }
                },
                CreatedAt = DateTime.UtcNow
            },
            new Component
            {
                Id = Guid.NewGuid(),
                Type = "number",
                Label = "Age",
                Required = true,
                Properties = new Dictionary<string, object>
                {
                    { "min", 18 },
                    { "max", 100 },
                    { "step", 1 }
                },
                CreatedAt = DateTime.UtcNow
            },
            new Component
            {
                Id = Guid.NewGuid(),
                Type = "select",
                Label = "Department",
                Required = true,
                Properties = new Dictionary<string, object>
                {
                    { "options", new[] { "IT", "HR", "Finance", "Marketing", "Operations", "Sales" } },
                    { "placeholder", "Select your department" }
                },
                CreatedAt = DateTime.UtcNow
            },
            new Component
            {
                Id = Guid.NewGuid(),
                Type = "checkbox",
                Label = "I agree to the terms and conditions",
                Required = true,
                Properties = new Dictionary<string, object>
                {
                    { "value", true }
                },
                CreatedAt = DateTime.UtcNow
            },
            new Component
            {
                Id = Guid.NewGuid(),
                Type = "textarea",
                Label = "Additional Comments",
                Required = false,
                Properties = new Dictionary<string, object>
                {
                    { "placeholder", "Please provide any additional comments" },
                    { "rows", 4 },
                    { "maxLength", 500 }
                },
                CreatedAt = DateTime.UtcNow
            },
            new Component
            {
                Id = Guid.NewGuid(),
                Type = "date",
                Label = "Start Date",
                Required = true,
                Properties = new Dictionary<string, object>
                {
                    { "min", DateTime.Now.ToString("yyyy-MM-dd") },
                    { "max", DateTime.Now.AddYears(1).ToString("yyyy-MM-dd") }
                },
                CreatedAt = DateTime.UtcNow
            },
            new Component
            {
                Id = Guid.NewGuid(),
                Type = "radio",
                Label = "Priority Level",
                Required = true,
                Properties = new Dictionary<string, object>
                {
                    { "options", new[] { "Low", "Medium", "High", "Urgent" } }
                },
                CreatedAt = DateTime.UtcNow
            }
        };

            dbContext.Components.AddRange(components);
            dbContext.SaveChanges();
            return components;
        }

        private static List<FormTemplate> CreateFormTemplates(AppDbContext dbContext)
        {
            var formTemplates = new List<FormTemplate>
        {
            new FormTemplate
            {
                Id = Guid.NewGuid(),
                Name = "Employee Registration",
                Content = "Form for registering new employees in the company system. This form collects basic information including personal details, department assignment, and agreement to company policies.",
                CreatedAt = DateTime.UtcNow
            },
            new FormTemplate
            {
                Id = Guid.NewGuid(),
                Name = "Project Request",
                Content = "Form for submitting new project requests. Include project details, timeline, budget requirements, and expected outcomes. This form will be reviewed by management for approval.",
                CreatedAt = DateTime.UtcNow
            },
            new FormTemplate
            {
                Id = Guid.NewGuid(),
                Name = "Equipment Request",
                Content = "Form for requesting office equipment and supplies. Specify the type of equipment needed, justification for the request, and urgency level. All requests require manager approval.",
                CreatedAt = DateTime.UtcNow
            },
            new FormTemplate
            {
                Id = Guid.NewGuid(),
                Name = "Leave Application",
                Content = "Form for applying for leave from work. Include leave type, dates, reason, and emergency contact information. Submit at least 2 weeks in advance for planned leave.",
                CreatedAt = DateTime.UtcNow
            },
            new FormTemplate
            {
                Id = Guid.NewGuid(),
                Name = "Performance Review",
                Content = "Annual performance review form for employees. Self-assessment section followed by manager evaluation. Used for promotion considerations and development planning.",
                CreatedAt = DateTime.UtcNow
            }
        };

            dbContext.FormTemplates.AddRange(formTemplates);
            dbContext.SaveChanges();
            return formTemplates;
        }

        private static void CreateSampleFormResponses(AppDbContext dbContext, List<User> users,
            List<FormTemplate> formTemplates, List<Step> steps, List<Component> components)
        {
            var random = new Random();
            var sampleFormResponses = new List<FormResponse>();

            // Create multiple form responses for different combinations
            for (int i = 0; i < 20; i++)
            {
                var user = users[random.Next(users.Count)];
                var template = formTemplates[random.Next(formTemplates.Count)];
                var step = steps[random.Next(steps.Count)];

                // Create sample response fields using available components
                var responseFields = new Dictionary<Guid, object>();

                // Add responses for each component type
                if (components.Any())
                {
                    var selectedComponents = components.Take(random.Next(2, Math.Min(5, components.Count))).ToList();
                    foreach (var component in selectedComponents)
                    {
                        object responseValue = component.Type switch
                        {
                            "text" => GenerateRandomName(),
                            "email" => GenerateRandomEmail(),
                            "number" => random.Next(18, 65),
                            "select" => new[] { "IT", "HR", "Finance", "Marketing", "Operations", "Sales" }[random.Next(6)],
                            "checkbox" => random.Next(2) == 1,
                            "textarea" => GenerateRandomComment(),
                            "date" => DateTime.Now.AddDays(random.Next(1, 365)).ToString("yyyy-MM-dd"),
                            "radio" => new[] { "Low", "Medium", "High", "Urgent" }[random.Next(4)],
                            _ => $"Sample response for {component.Label}"
                        };
                        responseFields[component.Id] = responseValue;
                    }
                }

                var formResponse = new FormResponse
                {
                    Id = Guid.NewGuid(),
                    FormTemplateId = template.Id,
                    StepId = step.Id,
                    UserId = user.Id,
                    ResponseFields = responseFields,
                    RejectReason = random.Next(10) == 0 ? "Sample rejection reason" : null, // 10% chance of rejection
                    CreatedAt = DateTime.UtcNow.AddDays(-random.Next(30)), // Random date within last 30 days
                    UpdatedAt = random.Next(5) == 0 ? DateTime.UtcNow.AddDays(-random.Next(10)) : null
                };

                sampleFormResponses.Add(formResponse);
            }

            dbContext.FormResponses.AddRange(sampleFormResponses);
        }

        private static string GenerateRandomComment()
        {
            var comments = new[]
            {
            "This request is urgent and needed for the upcoming project deadline.",
            "Please process this application at your earliest convenience.",
            "I have discussed this with my team lead and received approval.",
            "This equipment is essential for my daily work responsibilities.",
            "Looking forward to contributing to this new initiative.",
            "I have completed all prerequisite training for this role.",
            "This request aligns with our department's quarterly objectives.",
            "I am available for any additional information if needed."
        };
            var random = new Random();
            return comments[random.Next(comments.Length)];
        }

        private static string GenerateRandomName()
        {
            var firstNames = new[] { "John", "Jane", "Michael", "Sarah", "David", "Emily", "Robert", "Emma", "James", "Lisa" };
            var lastNames = new[] { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez" };
            var random = new Random();
            return $"{firstNames[random.Next(firstNames.Length)]} {lastNames[random.Next(lastNames.Length)]}";
        }

        private static string GenerateRandomEmail()
        {
            var domains = new[] { "example.com", "test.com", "demo.org", "sample.net" };
            var random = new Random();
            var name = GenerateRandomName().Replace(" ", ".").ToLower();
            return $"{name}@{domains[random.Next(domains.Length)]}";
        }
    }
}
