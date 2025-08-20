using FlowManager.Client.Services;
using FlowManager.Shared.DTOs.Responses.FormTemplate;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Microsoft.JSInterop;

namespace FlowManager.Client.Pages
{
    public partial class BasicUser : ComponentBase
    {
        private string _activeTab = "MYFORMS";
        protected string? errorMessage;

        // Form selection modal state
        private bool showFormSelectionModal = false;
        private bool isLoadingTemplates = false;
        private List<FormTemplateResponseDto>? availableTemplates;

        private void SetActiveTab(string tab)
        {
            _activeTab = tab;
        }

        protected async Task Logout()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/logout");

            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

            var response = await Http.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("[Auth] Logout successful, notifying state provider");

                (CookieAuthStateProvider as CookieAuthStateProvider)?.NotifyUserLogout();

                Navigation.NavigateTo("/");
            }
            else
            {
                Console.WriteLine($"[Auth] Logout failed with status: {response.StatusCode}");
                errorMessage = "Logout failed. Please try again.";
            }
        }

        private async Task ShowFormSelectionModal()
        {
            showFormSelectionModal = true;
            isLoadingTemplates = true;
            StateHasChanged();

            try
            {
                Console.WriteLine("Starting to load form templates using queried endpoint...");

                // Folosesc endpoint-ul queried
                availableTemplates = await FormTemplateService.GetAllFormTemplatesAsync();

                Console.WriteLine($"Loaded {availableTemplates?.Count ?? 0} templates");

                if (availableTemplates?.Any() == true)
                {
                    foreach (var template in availableTemplates)
                    {
                        Console.WriteLine($"Template: {template.Name} (ID: {template.Id}) - Created: {template.CreatedAt}");
                    }
                }
                else
                {
                    Console.WriteLine("No templates found or availableTemplates is null");

                    // Încerc să verific direct API-ul
                    Console.WriteLine("Testing direct API call...");
                    var testResponse = await Http.GetAsync("api/formtemplates/queried");
                    Console.WriteLine($"Direct API response status: {testResponse.StatusCode}");

                    if (testResponse.IsSuccessStatusCode)
                    {
                        var testContent = await testResponse.Content.ReadAsStringAsync();
                        Console.WriteLine($"Direct API response content: {testContent}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading templates: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                await JSRuntime.InvokeVoidAsync("alert", $"Error loading form templates: {ex.Message}");
            }
            finally
            {
                isLoadingTemplates = false;
                StateHasChanged();
            }
        }

        private void CloseFormSelectionModal()
        {
            showFormSelectionModal = false;
            StateHasChanged();
        }

        private void SelectTemplate(FormTemplateResponseDto template)
        {
            Navigation.NavigateTo($"/fill-form/{template.Id}");
        }

        private void AddForm()
        {
            _ = ShowFormSelectionModal();
        }
    }
}