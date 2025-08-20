using FlowManager.Client.Services;
using FlowManager.Shared.DTOs.Responses.Component;
using FlowManager.Shared.DTOs.Responses.FormTemplate;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using System.Text.Json;

namespace FlowManager.Client.Pages
{
    public partial class FillForm : ComponentBase
    {
        [Parameter] public Guid TemplateId { get; set; }

        private FormTemplateResponseDto? formTemplate;
        private List<FormElement>? formElements;
        private List<ComponentResponseDto>? components;
        private bool isLoading = true;
        private bool isSubmitting = false;
        private Dictionary<Guid, string> responses = new();

        protected override async Task OnInitializedAsync()
        {
            await LoadFormTemplate();
        }

        private async Task LoadFormTemplate()
        {
            isLoading = true;
            try
            {
                formTemplate = await FormTemplateService.GetFormTemplateByIdAsync(TemplateId);
                if (formTemplate != null)
                {
                    await ParseFormContent();
                    await LoadComponents();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading form template: {ex.Message}");
            }
            finally
            {
                isLoading = false;
                StateHasChanged();
            }
        }

        private async Task ParseFormContent()
        {
            if (string.IsNullOrEmpty(formTemplate?.Content))
                return;

            try
            {
                var contentData = JsonSerializer.Deserialize<FormContent>(formTemplate.Content);
                formElements = contentData?.Elements?.ToList() ?? new List<FormElement>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing form content: {ex.Message}");
                formElements = new List<FormElement>();
            }
        }

        private async Task LoadComponents()
        {
            if (formTemplate?.Components?.Any() != true)
                return;

            try
            {
                // Corectez să folosesc formTemplateComponent.Id în loc de ComponentId
                var componentTasks = formTemplate.Components.Select(async formTemplateComponent =>
                {
                    try
                    {
                        // Folosesc Id-ul din FormTemplateComponentResponseDto
                        return await ComponentService.GetComponentByIdAsync(formTemplateComponent.Id);
                    }
                    catch
                    {
                        return null;
                    }
                });

                var componentResults = await Task.WhenAll(componentTasks);
                components = componentResults.Where(c => c != null).ToList()!;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading components: {ex.Message}");
                components = new List<ComponentResponseDto>();
            }
        }

        private void UpdateResponse(Guid componentId, string? value)
        {
            if (value != null)
            {
                responses[componentId] = value;
            }
            else
            {
                responses.Remove(componentId);
            }
        }

        private async Task SubmitForm()
        {
            isSubmitting = true;
            try
            {
                var requiredComponents = components?.Where(c => c.Required == true) ?? Enumerable.Empty<ComponentResponseDto>();
                var missingRequired = requiredComponents.Where(c => !responses.ContainsKey(c.Id) || string.IsNullOrWhiteSpace(responses[c.Id]));

                if (missingRequired.Any())
                {
                    await JSRuntime.InvokeVoidAsync("alert", "Please fill in all required fields.");
                    return;
                }

                var formResponseData = new
                {
                    FormTemplateId = TemplateId,
                    Responses = responses.Select(r => new { ComponentId = r.Key, Value = r.Value }).ToList(),
                    SubmittedAt = DateTime.UtcNow
                };

                var response = await Http.PostAsJsonAsync("api/formresponses", formResponseData);

                if (response.IsSuccessStatusCode)
                {
                    await JSRuntime.InvokeVoidAsync("alert", "Form submitted successfully!");
                    Navigation.NavigateTo("/basic-user");
                }
                else
                {
                    await JSRuntime.InvokeVoidAsync("alert", "Error submitting form. Please try again.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error submitting form: {ex.Message}");
                await JSRuntime.InvokeVoidAsync("alert", "Error submitting form. Please try again.");
            }
            finally
            {
                isSubmitting = false;
                StateHasChanged();
            }
        }

        private void GoBack()
        {
            Navigation.NavigateTo("/basic-user");
        }

        public class FormContent
        {
            public string Layout { get; set; } = "";
            public List<FormElement> Elements { get; set; } = new();
        }

        public class FormElement
        {
            public string Id { get; set; } = "";
            public int X { get; set; }
            public int Y { get; set; }
            public int ZIndex { get; set; }
            public bool IsTextElement { get; set; }
            public string? TextContent { get; set; }
            public Guid? ComponentId { get; set; }
            public string? ComponentType { get; set; }
            public string? Label { get; set; }
            public bool? Required { get; set; }
            public Dictionary<string, object>? Properties { get; set; }
        }
    }
}