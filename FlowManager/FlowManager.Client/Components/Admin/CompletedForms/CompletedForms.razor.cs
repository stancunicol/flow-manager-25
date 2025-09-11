using FlowManager.Client.DTOs;
using FlowManager.Client.Services;
using FlowManager.Shared.DTOs.Requests.FormResponse;
using FlowManager.Shared.DTOs.Responses;
using FlowManager.Shared.DTOs.Responses.Component;
using FlowManager.Shared.DTOs.Responses.Flow;
using FlowManager.Shared.DTOs.Responses.FormTemplate;
using FlowManager.Shared.DTOs.Responses.Step;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using System.Text.Json;

namespace FlowManager.Client.Components.Admin.CompletedForms
{
    public partial class CompletedForms : ComponentBase, IDisposable
    {
        // Status filter state
        private HashSet<string> selectedStatuses = new HashSet<string> { "Pending", "Rejected", "Approved" };

        // Completed forms state with search and pagination
        private bool isLoadingForms = false;
        private List<FormResponseResponseDto>? completedForms;
        private string searchTerm = "";
        private Timer? searchDebounceTimer;

        // Pagination state
        private int _pageSize = 12; // Slightly more for admin view
        private int _currentPage = 1;
        private int _totalPages = 0;
        private int _maxVisiblePages = 5;
        private int _totalCount = 0;

        // View Form modal state
        private bool showViewFormModal = false;
        private bool isLoadingFormDetails = false;
        private bool isLoadingFlowSteps = false;
        private List<StepResponseDto>? flowSteps;
        private FormResponseResponseDto? selectedFormResponse;
        private FormTemplateResponseDto? selectedFormTemplate;
        private List<ComponentResponseDto>? formComponents;
        private List<FormElement>? formElements;

        [Inject] protected FormResponseService FormResponseService { get; set; } = default!;
        [Inject] protected FormTemplateService FormTemplateService { get; set; } = default!;
        [Inject] protected ComponentService ComponentService { get; set; } = default!;
        [Inject] protected HttpClient Http { get; set; } = default!;
        [Inject] protected IJSRuntime JSRuntime { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            await LoadCompletedForms();
        }

        private async Task LoadCompletedForms()
        {
            isLoadingForms = true;
            StateHasChanged();

            try
            {
                Console.WriteLine($"[CompletedForms] Loading all completed forms - Page: {_currentPage}, PageSize: {_pageSize}, Search: '{searchTerm}', Statuses: [{string.Join(", ", selectedStatuses)}]");

                var queryParams = new QueriedFormResponseRequestDto
                {
                    QueryParams = new()
                    {
                        Page = _currentPage,
                        PageSize = _pageSize
                    },
                    SearchTerm = string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm,
                    StatusFilters = selectedStatuses.ToList()
                };

                var response = await FormResponseService.GetAllFormResponsesQueriedAsync(queryParams);

                if (response != null)
                {
                    completedForms = response.Data?.ToList() ?? new List<FormResponseResponseDto>();
                    _totalCount = response.TotalCount;
                    _totalPages = (int)Math.Ceiling((double)_totalCount / _pageSize);

                    Console.WriteLine($"[CompletedForms] Loaded {completedForms.Count} completed forms (page {_currentPage}/{_totalPages}, total: {_totalCount})");
                }
                else
                {
                    Console.WriteLine("[CompletedForms] Failed to load forms - response was null");
                    completedForms = new List<FormResponseResponseDto>();
                    _totalCount = 0;
                    _totalPages = 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CompletedForms] Error loading completed forms: {ex.Message}");
                completedForms = new List<FormResponseResponseDto>();
                _totalCount = 0;
                _totalPages = 0;
                await JSRuntime.InvokeVoidAsync("alert", $"Error loading completed forms: {ex.Message}");
            }
            finally
            {
                isLoadingForms = false;
                StateHasChanged();
            }
        }

        // Status filtering methods
        private async Task FilterByStatus(string status)
        {
            selectedStatuses = new HashSet<string> { status };
            _currentPage = 1;
            await LoadCompletedForms();
        }

        private async Task ShowAllStatuses()
        {
            selectedStatuses = new HashSet<string> { "Pending", "Rejected", "Approved" };
            _currentPage = 1;
            await LoadCompletedForms();
        }

        private bool IsOnlyStatusSelected(string status)
        {
            return selectedStatuses.Count == 1 && selectedStatuses.Contains(status);
        }

        private bool AreAllStatusesSelected()
        {
            return selectedStatuses.Count == 3 &&
                   selectedStatuses.Contains("Pending") &&
                   selectedStatuses.Contains("Rejected") &&
                   selectedStatuses.Contains("Approved");
        }

        // Search functionality
        private void OnSearchInput(ChangeEventArgs e)
        {
            var newSearchTerm = e.Value?.ToString() ?? "";

            searchDebounceTimer?.Dispose();
            searchDebounceTimer = new Timer(async _ =>
            {
                await InvokeAsync(async () =>
                {
                    searchTerm = newSearchTerm;
                    _currentPage = 1;
                    await LoadCompletedForms();
                });
            }, null, 500, Timeout.Infinite);
        }

        private async Task ClearSearch()
        {
            searchTerm = "";
            _currentPage = 1;
            await LoadCompletedForms();
        }

        // Pagination methods
        private async Task GoToFirstPage()
        {
            _currentPage = 1;
            await LoadCompletedForms();
        }

        private async Task GoToPreviousPage()
        {
            _currentPage--;
            await LoadCompletedForms();
        }

        private List<int> GetPageNumbers()
        {
            List<int> pages = new List<int>();
            int half = (int)Math.Floor(_maxVisiblePages / 2.0);
            int start = Math.Max(1, _currentPage - half);
            int end = Math.Min(_totalPages, start + _maxVisiblePages - 1);

            if (end - start + 1 < _maxVisiblePages)
            {
                start = Math.Max(1, end - _maxVisiblePages + 1);
            }

            for (int i = start; i <= end; i++)
            {
                pages.Add(i);
            }

            return pages;
        }

        private async Task GoToPage(int page)
        {
            _currentPage = page;
            await LoadCompletedForms();
        }

        private async Task GoToNextPage()
        {
            _currentPage++;
            await LoadCompletedForms();
        }

        private async Task GoToLastPage()
        {
            _currentPage = _totalPages;
            await LoadCompletedForms();
        }

        private async Task RefreshForms()
        {
            Console.WriteLine("[CompletedForms] Refreshing completed forms...");
            _currentPage = 1;
            await LoadCompletedForms();
        }

        // View form details
        private async Task ViewFormResponse(FormResponseResponseDto formResponse)
        {
            selectedFormResponse = formResponse;
            showViewFormModal = true;
            isLoadingFormDetails = true;
            isLoadingFlowSteps = true;
            StateHasChanged();

            try
            {
                selectedFormTemplate = await FormTemplateService.GetFormTemplateByIdAsync(formResponse.FormTemplateId);

                if (selectedFormTemplate != null)
                {
                    await ParseFormContent();
                    await LoadFormComponents();
                    await LoadFlowSteps();
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
                isLoadingFlowSteps = false;
                StateHasChanged();
            }
        }

        private async Task LoadFlowSteps()
        {
            if (selectedFormResponse == null)
            {
                flowSteps = new List<StepResponseDto>();
                return;
            }

            try
            {
                isLoadingFlowSteps = true;

                var stepResponse = await Http.GetAsync($"api/steps/{selectedFormResponse.StepId}");
                if (stepResponse.IsSuccessStatusCode)
                {
                    var stepApiResponse = await stepResponse.Content.ReadFromJsonAsync<ApiResponse<StepResponseDto>>();
                    var step = stepApiResponse?.Result;

                    if (step != null)
                    {
                        if (selectedFormTemplate?.FlowId != null)
                        {
                            var flowStepsResponse = await Http.GetAsync($"api/flows/{selectedFormTemplate.FlowId}/steps");
                            if (flowStepsResponse.IsSuccessStatusCode)
                            {
                                var flowStepsApiResponse = await flowStepsResponse.Content.ReadFromJsonAsync<ApiResponse<List<StepResponseDto>>>();
                                flowSteps = flowStepsApiResponse?.Result ?? new List<StepResponseDto>();
                                return;
                            }
                        }

                        var flowsResponse = await Http.GetAsync("api/flows/queried?QueryParams.PageSize=100");
                        if (flowsResponse.IsSuccessStatusCode)
                        {
                            var flowsApiResponse = await flowsResponse.Content.ReadFromJsonAsync<ApiResponse<PagedResponseDto<FlowResponseDto>>>();
                            var flows = flowsApiResponse?.Result?.Data;

                            if (flows?.Any() == true)
                            {
                                var matchingFlow = flows.FirstOrDefault(f => f.Steps?.Any(s => s.Id == selectedFormResponse.StepId) == true);
                                if (matchingFlow != null)
                                {
                                    var flowStepsResponse = await Http.GetAsync($"api/flows/{matchingFlow.Id}/steps");
                                    if (flowStepsResponse.IsSuccessStatusCode)
                                    {
                                        var flowStepsApiResponse = await flowStepsResponse.Content.ReadFromJsonAsync<ApiResponse<List<StepResponseDto>>>();
                                        flowSteps = flowStepsApiResponse?.Result ?? new List<StepResponseDto>();
                                    }
                                    else
                                    {
                                        flowSteps = matchingFlow.Steps ?? new List<StepResponseDto>();
                                    }
                                }
                                else
                                {
                                    flowSteps = new List<StepResponseDto> { step };
                                }
                            }
                        }
                    }
                }
                else
                {
                    flowSteps = new List<StepResponseDto>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading flow steps: {ex.Message}");
                flowSteps = new List<StepResponseDto>();
            }
            finally
            {
                isLoadingFlowSteps = false;
                StateHasChanged();
            }
        }

        private string GetFormStatus(FormResponseResponseDto form)
        {
            if (!string.IsNullOrEmpty(form.Status))
            {
                return form.Status;
            }
            if (!string.IsNullOrEmpty(form.RejectReason))
            {
                return "Rejected";
            }
            return "Pending";
        }

        private string GetStatusColor(string status)
        {
            return status switch
            {
                "Pending" => "#f59e0b",
                "Rejected" => "#ef4444",
                "Approved" => "#10b981",
                _ => "#6b7280"
            };
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
            flowSteps = null;
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

        public void Dispose()
        {
            searchDebounceTimer?.Dispose();
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