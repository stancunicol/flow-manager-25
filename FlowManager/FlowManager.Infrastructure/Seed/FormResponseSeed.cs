using FlowManager.Domain.Entities;
using FlowManager.Infrastructure.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Infrastructure.Seed
{
    public static class FormResponseSeed
    {
        public static void Populate(AppDbContext dbContext)
        {
            // Verify we have the required data first
            var users = dbContext.Users.ToList();
            var formTemplates = dbContext.FormTemplates.ToList();
            var steps = dbContext.Steps.ToList();
            var components = dbContext.Components.ToList();

            if (!users.Any() || !formTemplates.Any() || !steps.Any())
            {
                // Create basic dependencies if they don't exist
                CreateBasicDependencies(dbContext);
                dbContext.SaveChanges();

                // Refresh the lists
                users = dbContext.Users.ToList();
                formTemplates = dbContext.FormTemplates.ToList();
                steps = dbContext.Steps.ToList();
                components = dbContext.Components.ToList();
            }

            // Create sample FormResponses
            CreateSampleFormResponses(dbContext, users, formTemplates, steps, components);

            dbContext.SaveChanges();
        }

        private static void CreateBasicDependencies(AppDbContext dbContext)
        {
            // Create Steps if they don't exist
            if (!dbContext.Steps.Any())
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
            }

            // Create Components if they don't exist
            if (!dbContext.Components.Any())
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
            }

            // Create FormTemplates if they don't exist (respecting MaxLength constraints)
            if (!dbContext.FormTemplates.Any())
            {
                var formTemplates = new List<FormTemplate>
                {
                    new FormTemplate
                    {
                        Id = Guid.NewGuid(),
                        Name = "Employee Registration", // MaxLength 50
                        Content = "Form for registering new employees in the company system. This form collects basic information including personal details, department assignment, and agreement to company policies.", // MaxLength 1000
                        CreatedAt = DateTime.UtcNow
                    },
                    new FormTemplate
                    {
                        Id = Guid.NewGuid(),
                        Name = "Project Request", // MaxLength 50
                        Content = "Form for submitting new project requests. Include project details, timeline, budget requirements, and expected outcomes. This form will be reviewed by management for approval.", // MaxLength 1000
                        CreatedAt = DateTime.UtcNow
                    },
                    new FormTemplate
                    {
                        Id = Guid.NewGuid(),
                        Name = "Equipment Request", // MaxLength 50
                        Content = "Form for requesting office equipment and supplies. Specify the type of equipment needed, justification for the request, and urgency level. All requests require manager approval.", // MaxLength 1000
                        CreatedAt = DateTime.UtcNow
                    },
                    new FormTemplate
                    {
                        Id = Guid.NewGuid(),
                        Name = "Leave Application", // MaxLength 50
                        Content = "Form for applying for leave from work. Include leave type, dates, reason, and emergency contact information. Submit at least 2 weeks in advance for planned leave.", // MaxLength 1000
                        CreatedAt = DateTime.UtcNow
                    },
                    new FormTemplate
                    {
                        Id = Guid.NewGuid(),
                        Name = "Performance Review", // MaxLength 50
                        Content = "Annual performance review form for employees. Self-assessment section followed by manager evaluation. Used for promotion considerations and development planning.", // MaxLength 1000
                        CreatedAt = DateTime.UtcNow
                    }
                };
                dbContext.FormTemplates.AddRange(formTemplates);
            }
        }

        private static void CreateSampleFormResponses(AppDbContext dbContext, List<User> users, List<FormTemplate> formTemplates, List<Step> steps, List<Component> components)
        {
            // Check if FormResponses already exist
            if (dbContext.FormResponses.Any())
                return;

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

