using FlowManager.Client.Services;
using FlowManager.Shared.DTOs;
using FlowManager.Shared.DTOs.Requests.FormResponse;
using FlowManager.Shared.DTOs.Responses.Component;
using FlowManager.Shared.DTOs.Responses.FormTemplate;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using static FlowManager.Client.Pages.FillForm;

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

        // User forms state
        private bool isLoadingUserForms = false;
        private List<FormResponseResponseDto>? userForms;
        private Guid currentUserId = Guid.Empty;

        //View Form modal state
        private bool showViewFormModal = false;
        private bool isLoadingFormDetails = false;
        private FormResponseResponseDto? selectedFormResponse;
        private FormTemplateResponseDto? selectedFormTemplate;
        private List<ComponentResponseDto>? formComponents;
        private List<FormElement>? formElements;

        [Inject] protected FormTemplateService FormTemplateService { get; set; } = default!;
        [Inject] protected FormResponseService FormResponseService { get; set; } = default!;
        [Inject] protected ComponentService ComponentService { get; set; } = default!;
        [Inject] protected NavigationManager Navigation { get; set; } = default!;
        [Inject] protected AuthenticationStateProvider AuthProvider { get; set; } = default!;
        [Inject] protected CookieAuthStateProvider CookieAuthStateProvider { get; set; } = default!;
        [Inject] protected HttpClient Http { get; set; } = default!;
        [Inject] protected IJSRuntime JSRuntime { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthProvider.GetAuthenticationStateAsync();

            if (!authState.User.Identity?.IsAuthenticated ?? true)
            {
                Navigation.NavigateTo("/");
                return;
            }

            if (!authState.User.IsInRole("Basic"))
            {
                Navigation.NavigateTo("/");
                return;
            }

            await LoadCurrentUser();
            if (currentUserId != Guid.Empty)
            {
                await LoadUserForms();
            }
        }

        private async Task LoadCurrentUser()
        {
            try
            {
                var response = await Http.GetAsync("api/auth/me");
                if (response.IsSuccessStatusCode)
                {
                    var userInfo = await response.Content.ReadFromJsonAsync<UserProfileDto>();
                    if (userInfo != null && userInfo.Id != Guid.Empty)
                    {
                        currentUserId = userInfo.Id;
                        Console.WriteLine($"[DEBUG] Current user ID: {currentUserId}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to load current user: {ex.Message}");
            }
        }

        private async Task LoadUserForms()
        {
            isLoadingUserForms = true;
            StateHasChanged();

            try
            {
                userForms = await FormResponseService.GetFormResponsesByUserAsync(currentUserId);
                Console.WriteLine($"Loaded {userForms?.Count ?? 0} forms for user {currentUserId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading user forms: {ex.Message}");
            }
            finally
            {
                isLoadingUserForms = false;
                StateHasChanged();
            }
        }


        private async Task ViewFormResponse(FormResponseResponseDto formResponse)
        {
            selectedFormResponse = formResponse;
            showViewFormModal = true;
            isLoadingFormDetails = true;
            StateHasChanged();

            try
            {
                // Încarcă template-ul formularului
                selectedFormTemplate = await FormTemplateService.GetFormTemplateByIdAsync(formResponse.FormTemplateId);

                if (selectedFormTemplate != null)
                {
                    // Parsează conținutul formularului
                    await ParseFormContent();

                    // Încarcă componentele
                    await LoadFormComponents();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading form details: {ex.Message}");
                await JSRuntime.InvokeVoidAsync("alert", $"Error loading form details: {ex.Message}");
            }
            finally
            {
                isLoadingFormDetails = false;
                StateHasChanged();
            }
        }

        private async Task ParseFormContent()
        {
            if (string.IsNullOrEmpty(selectedFormTemplate?.Content))
                return;

            try
            {
                var contentData = JsonSerializer.Deserialize<FormContent>(selectedFormTemplate.Content);
                formElements = contentData?.Elements?.ToList() ?? new List<FormElement>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing form content: {ex.Message}");
                formElements = new List<FormElement>();
            }
        }

        private async Task LoadFormComponents()
        {
            if (selectedFormTemplate?.Components?.Any() != true)
                return;

            try
            {
                var componentTasks = selectedFormTemplate.Components.Select(async formTemplateComponent =>
                {
                    try
                    {
                        return await ComponentService.GetComponentByIdAsync(formTemplateComponent.Id);
                    }
                    catch
                    {
                        return null;
                    }
                });

                var componentResults = await Task.WhenAll(componentTasks);
                formComponents = componentResults.Where(c => c != null).ToList()!;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading components: {ex.Message}");
                formComponents = new List<ComponentResponseDto>();
            }
        }

        private void CloseViewFormModal()
        {
            showViewFormModal = false;
            selectedFormResponse = null;
            selectedFormTemplate = null;
            formComponents = null;
            formElements = null;
            StateHasChanged();
        }

        private object? GetFieldValue(Guid componentId)
        {
            if (selectedFormResponse?.ResponseFields?.ContainsKey(componentId) == true)
            {
                return selectedFormResponse.ResponseFields[componentId];
            }
            return null;
        }

        private string FormatFieldValue(object? value, string componentType)
        {
            if (value == null) return "No response";

            return componentType.ToLower() switch
            {
                "checkbox" => value.ToString() == "True" ? "Yes" : "No",
                "datetime" => DateTime.TryParse(value.ToString(), out var date) ? date.ToString("dd/MM/yyyy HH:mm") : value.ToString()!,
                _ => value.ToString() ?? "No response"
            };
        }


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
                availableTemplates = await FormTemplateService.GetAllFormTemplatesAsync();
                Console.WriteLine($"Loaded {availableTemplates?.Count ?? 0} templates");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading templates: {ex.Message}");
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

        private async Task SelectTemplate(FormTemplateResponseDto template)
        {
            Navigation.NavigateTo($"/fill-form/{template.Id}");
        }

        private void AddForm()
        {
            _ = ShowFormSelectionModal();
        }

        private async Task RefreshUserForms()
        {
            await LoadUserForms();
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