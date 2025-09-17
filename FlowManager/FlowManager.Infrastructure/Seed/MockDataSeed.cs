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

            // Create users with balanced role distribution
            var users = CreateUsers(dbContext, passwordHasher, roles, steps);

            // Create balanced teams
            var teams = CreateBalancedTeams(dbContext, steps);

            // Create balanced user-team relationships
            CreateBalancedUserTeamRelationships(dbContext, users, teams, steps);

            // Create components
            var components = CreateComponents(dbContext);


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
            // Generate 50 users with balanced distribution
            var firstNames = new[] { "James", "Sarah", "Michael", "Emily", "David", "Jessica", "Robert", "Amanda", "Christopher", "Nicole",
                            "Kevin", "Lauren", "Daniel", "Melissa", "Andrew", "Ashley", "Matthew", "Stephanie", "Ryan", "Rachel",
                            "Brandon", "Samantha", "Justin", "Brittany", "John", "Michelle", "Anthony", "Danielle", "William", "Katherine",
                            "Joshua", "Amy", "Nicholas", "Angela", "Tyler", "Heather", "Alexander", "Rebecca", "Jonathan", "Jennifer",
                            "Nathan", "Elizabeth", "Patrick", "Maria", "Jason", "Lisa", "Adam", "Christine", "Mark", "Laura" };

            var lastNames = new[] { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez",
                           "Hernandez", "Lopez", "Gonzalez", "Wilson", "Anderson", "Thomas", "Taylor", "Moore", "Jackson", "Martin",
                           "Lee", "Perez", "Thompson", "White", "Harris", "Sanchez", "Clark", "Ramirez", "Lewis", "Robinson",
                           "Walker", "Young", "Allen", "King", "Wright", "Scott", "Torres", "Nguyen", "Hill", "Flores",
                           "Green", "Adams", "Nelson", "Baker", "Hall", "Rivera", "Campbell", "Mitchell", "Carter", "Roberts" };

            var createdUsers = new List<User>();

            // Create balanced role distribution across all steps
            // 5 steps × 10 users per step = 50 users total
            // Per step: 1 admin, 3 moderators, 6 basic users
            var usersPerStep = 50 / steps.Count; // 10 users per step
            var adminsPerStep = 1;
            var moderatorsPerStep = 3;
            var basicPerStep = 6;

            int userIndex = 0;
            for (int stepIndex = 0; stepIndex < steps.Count; stepIndex++)
            {
                var step = steps[stepIndex];

                // Create role distribution for this step
                var stepRoleDistribution = new List<(bool IsAdmin, bool IsModerator)>();

                // Add 1 admin for this step
                stepRoleDistribution.Add((true, false));

                // Add 3 moderators for this step
                for (int j = 0; j < moderatorsPerStep; j++)
                    stepRoleDistribution.Add((false, true));

                // Add 6 basic users for this step
                for (int j = 0; j < basicPerStep; j++)
                    stepRoleDistribution.Add((false, false));

                // Create users for this step
                for (int i = 0; i < usersPerStep && userIndex < 50; i++)
                {
                    var userName = $"{firstNames[userIndex % firstNames.Length].ToLower()}{lastNames[userIndex % lastNames.Length].ToLower()}";
                    var name = $"{firstNames[userIndex % firstNames.Length]} {lastNames[userIndex % lastNames.Length]}";
                    var email = $"{userName}@company.com";

                    User? existingUser = dbContext.Users.FirstOrDefault(u => u.NormalizedUserName == userName.ToUpper());
                    if (existingUser == null)
                    {
                        var (isAdmin, isModerator) = stepRoleDistribution[i];

                        var user = new User
                        {
                            Id = Guid.NewGuid(),
                            UserName = userName,
                            NormalizedUserName = userName.ToUpper(),
                            Email = email,
                            NormalizedEmail = email.ToUpper(),
                            EmailConfirmed = true,
                            Name = name,
                            StepId = step.Id, // Assign user to specific step
                            SecurityStamp = Guid.NewGuid().ToString(),
                            ConcurrencyStamp = Guid.NewGuid().ToString(),
                            // Add phone number from Identity base entity
                            PhoneNumber = GenerateRandomPhoneNumber(),
                            PhoneNumberConfirmed = true // Mark phone as confirmed
                        };

                        user.PasswordHash = passwordHasher.HashPassword(user, "password123");
                        dbContext.Users.Add(user);
                        createdUsers.Add(user);

                        // Everyone gets basic role
                        dbContext.UserRoles.Add(new UserRole
                        {
                            UserId = user.Id,
                            RoleId = roles.basicRole.Id
                        });

                        // Add moderator role if applicable
                        if (isModerator)
                        {
                            dbContext.UserRoles.Add(new UserRole
                            {
                                UserId = user.Id,
                                RoleId = roles.moderatorRole.Id
                            });
                        }

                        // Add admin role if applicable
                        if (isAdmin)
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

                    userIndex++;
                }
            }

            dbContext.SaveChanges();
            return createdUsers;
        }

        // Helper method to generate random Romanian phone numbers
        private static string GenerateRandomPhoneNumber()
        {
            var random = new Random();

            // Romanian mobile operators with their real prefixes
            var mobileOperators = new Dictionary<string, string[]>
        {
            // Orange Romania
            { "Orange", new[] { "740", "741", "742", "743", "744", "745", "746", "747", "748" } },
            
            // Vodafone Romania
            { "Vodafone", new[] { "720", "721", "722", "723", "724", "725", "726", "727", "728", "729",
                                 "750", "751", "752", "753", "754", "755", "756", "757", "758", "759" } },
            
            // Telekom Romania
            { "Telekom", new[] { "730", "731", "732", "733", "734", "735", "736", "737", "738", "739" } },
            
            // Digi Mobil
            { "Digi", new[] { "760", "761", "762", "763", "764", "765", "766", "767", "768", "769" } },
            
            // RCS & RDS
            { "RCS", new[] { "770", "771", "772", "773", "774", "775", "776", "777", "778", "779" } }
        };

            // Get all prefixes from all operators
            var allPrefixes = mobileOperators.Values.SelectMany(x => x).ToArray();

            // Select random prefix
            var selectedPrefix = allPrefixes[random.Next(allPrefixes.Length)];

            // Generate remaining 6 digits (XXX XXX format)
            var middlePart = random.Next(100, 999).ToString("D3");
            var lastPart = random.Next(100, 999).ToString("D3");

            return $"+40 {selectedPrefix} {middlePart} {lastPart}";
        }

        private static List<Team> CreateBalancedTeams(AppDbContext dbContext, List<Step> steps)
        {
            var createdTeams = new List<Team>();

            // Create 2 teams per step (25 users per step / ~5-6 users per team = 2 teams per step)
            foreach (var step in steps)
            {
                for (int teamNumber = 1; teamNumber <= 2; teamNumber++)
                {
                    string teamName = $"{step.Name} Team {teamNumber}";

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
                }
            }

            dbContext.SaveChanges();
            return createdTeams;
        }

        private static void CreateBalancedUserTeamRelationships(AppDbContext dbContext, List<User> users,
            List<Team> teams, List<Step> steps)
        {
            // Group users by their step assignment
            var usersByStep = users.GroupBy(u => u.StepId).ToList();

            foreach (var userGroup in usersByStep)
            {
                var stepId = userGroup.Key;
                var stepUsers = userGroup.ToList();
                var step = steps.First(s => s.Id == stepId);

                // Get teams for this step
                var stepTeams = teams.Where(t => t.Name.Contains(step.Name)).ToList();

                // Sort users by role to ensure balanced distribution
                // Admins first, then moderators, then basic users
                var sortedUsers = stepUsers.OrderBy(u => {
                    var userRoles = dbContext.UserRoles.Where(ur => ur.UserId == u.Id).ToList();
                    bool isAdmin = userRoles.Any(ur => ur.RoleId == dbContext.Roles.First(r => r.NormalizedName == "ADMIN").Id);
                    bool isModerator = userRoles.Any(ur => ur.RoleId == dbContext.Roles.First(r => r.NormalizedName == "MODERATOR").Id);

                    if (isAdmin) return 0;
                    if (isModerator) return 1;
                    return 2;
                }).ToList();

                // Distribute users evenly across teams in round-robin fashion
                for (int i = 0; i < sortedUsers.Count; i++)
                {
                    var user = sortedUsers[i];
                    var targetTeam = stepTeams[i % stepTeams.Count]; // Round-robin assignment

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
                    FlowStepId = template.ActiveFlow.FlowSteps.First().Id,   
                    UserId = user.Id,
                    ResponseFields = responseFields,
                    RejectReason = random.Next(10) == 0 ? "Sample rejection reason" : null,
                    CreatedAt = DateTime.UtcNow.AddDays(-random.Next(30)),
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
