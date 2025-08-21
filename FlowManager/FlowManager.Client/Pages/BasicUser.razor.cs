using FlowManager.Client.Services;
using FlowManager.Shared.DTOs;
using FlowManager.Shared.DTOs.Requests.FormResponse;
using FlowManager.Shared.DTOs.Responses.FormTemplate;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using System.Security.Claims;

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

        protected override async Task OnInitializedAsync()
        {
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
    }
}