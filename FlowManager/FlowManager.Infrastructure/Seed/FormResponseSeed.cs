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
            var flows = dbContext.Flows.ToList();
            var formTemplates = dbContext.FormTemplates.ToList();
            var steps = dbContext.Steps.ToList();
            var components = dbContext.Components.ToList();

            if (!users.Any() || !flows.Any() || !steps.Any())
            {
                // Create basic dependencies if they don't exist
                CreateBasicDependencies(dbContext);
                dbContext.SaveChanges();

                // Refresh the lists
                users = dbContext.Users.ToList();
                flows = dbContext.Flows.ToList();
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
                        Name = "Initial Review",
                        CreatedAt = DateTime.UtcNow
                    },
                    new Step
                    {
                        Id = Guid.NewGuid(),
                        Name = "Manager Approval",
                        CreatedAt = DateTime.UtcNow
                    },
                    new Step
                    {
                        Id = Guid.NewGuid(),
                        Name = "Final Processing",
                        CreatedAt = DateTime.UtcNow
                    },
                    new Step
                    {
                        Id = Guid.NewGuid(),
                        Name = "HR Review",
                        CreatedAt = DateTime.UtcNow
                    },
                    new Step
                    {
                        Id = Guid.NewGuid(),
                        Name = "Completed",
                        CreatedAt = DateTime.UtcNow
                    }
                };
                dbContext.Steps.AddRange(steps);
                dbContext.SaveChanges(); // Save steps first
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
                dbContext.SaveChanges(); // Save components
            }

            // Create Flows if they don't exist
            if (!dbContext.Flows.Any())
            {
                var flows = new List<Flow>
                {
                    new Flow
                    {
                        Id = Guid.NewGuid(),
                        Name = "Employee Onboarding Flow",
                        CreatedAt = DateTime.UtcNow
                    },
                    new Flow
                    {
                        Id = Guid.NewGuid(),
                        Name = "Project Approval Flow",
                        CreatedAt = DateTime.UtcNow
                    },
                    new Flow
                    {
                        Id = Guid.NewGuid(),
                        Name = "Equipment Request Flow",
                        CreatedAt = DateTime.UtcNow
                    },
                    new Flow
                    {
                        Id = Guid.NewGuid(),
                        Name = "Leave Management Flow",
                        CreatedAt = DateTime.UtcNow
                    },
                    new Flow
                    {
                        Id = Guid.NewGuid(),
                        Name = "Performance Review Flow",
                        CreatedAt = DateTime.UtcNow
                    }
                };
                dbContext.Flows.AddRange(flows);
                dbContext.SaveChanges(); // Save flows first

                // Now create FormTemplates for each Flow
                var savedFlows = dbContext.Flows.ToList();
                var savedComponents = dbContext.Components.ToList();
                CreateFormTemplatesForFlows(dbContext, savedFlows, savedComponents);
            }
        }

        private static void CreateFormTemplatesForFlows(AppDbContext dbContext, List<Flow> flows, List<Component> components)
        {
            var random = new Random();

            foreach (var flow in flows)
            {
                // Create 1-2 FormTemplate versions for each Flow
                var templateCount = random.Next(1, 3); // 1 or 2 templates per flow

                for (int i = 0; i < templateCount; i++)
                {
                    var templateName = flow.Name.Replace(" Flow", "") + (i == 0 ? " Form" : $" Form v{i + 1}");

                    var formTemplate = new FormTemplate
                    {
                        Id = Guid.NewGuid(),
                        Name = templateName.Length > 50 ? templateName.Substring(0, 50) : templateName,
                        Content = GenerateFormContent(flow.Name),
                        CreatedAt = DateTime.UtcNow.AddDays(-i * 5) // Older versions have earlier dates
                    };

                    formTemplate.FormTemplateFlows.Add(new FormTemplateFlow
                    {
                        FlowId = flow.Id,
                        FormTemplateId = formTemplate.Id,
                    });

                    dbContext.FormTemplates.Add(formTemplate);
                    dbContext.SaveChanges(); // Save template to get ID

                    // Add some random components to this template
                    var selectedComponents = components.Take(random.Next(3, 6)).ToList();
                    foreach (var component in selectedComponents)
                    {
                        var formTemplateComponent = new FormTemplateComponent
                        {
                            Id = Guid.NewGuid(),
                            FormTemplateId = formTemplate.Id,
                            ComponentId = component.Id,
                            CreatedAt = DateTime.UtcNow
                        };
                        dbContext.FormTemplateComponents.Add(formTemplateComponent);
                    }
                }
            }

            dbContext.SaveChanges();
        }

        private static string GenerateFormContent(string flowName)
        {
            var contentTemplates = new Dictionary<string, string>
            {
                ["Employee Onboarding Flow"] = "Form for registering new employees. Collects personal information, department assignment, and policy agreements.",
                ["Project Approval Flow"] = "Form for submitting project requests. Include details, timeline, budget, and expected outcomes for management review.",
                ["Equipment Request Flow"] = "Form for requesting office equipment. Specify equipment type, justification, and urgency level for approval.",
                ["Leave Management Flow"] = "Form for leave applications. Include dates, type, reason, and emergency contacts. Submit 2 weeks in advance.",
                ["Performance Review Flow"] = "Annual performance review form with self-assessment and manager evaluation sections for development planning."
            };

            return contentTemplates.ContainsKey(flowName)
                ? contentTemplates[flowName]
                : "Standard form template for workflow processing and approval.";
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

                // Get components associated with this template
                var templateComponents = dbContext.FormTemplateComponents
                    .Where(ftc => ftc.FormTemplateId == template.Id && ftc.DeletedAt == null)
                    .Select(ftc => ftc.ComponentId)
                    .ToList();

                var responseFields = new Dictionary<Guid, object>();

                // Add responses for components in this template
                foreach (var componentId in templateComponents)
                {
                    var component = components.FirstOrDefault(c => c.Id == componentId);
                    if (component == null) continue;

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

                var formResponse = new FormResponse
                {
                    Id = Guid.NewGuid(),
                    FormTemplateId = template.Id,
                    StepId = step.Id,
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