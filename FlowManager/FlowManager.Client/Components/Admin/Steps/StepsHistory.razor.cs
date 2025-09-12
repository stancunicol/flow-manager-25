using FlowManager.Shared.DTOs.Responses.StepHistory;
using Microsoft.AspNetCore.Components;
using FlowManager.Client.Services;
using FlowManager.Shared.DTOs.Requests.StepHistory;
using QueryParams = FlowManager.Shared.DTOs.Requests.QueryParamsDto;

namespace FlowManager.Client.Components.Admin.Steps
{
    public partial class StepsHistory: ComponentBase
    {
        List<StepHistoryResponseDto> history = new();
        private List<string> messages = new();
        private int pageSize = 10;
        private int currentPage = 1;
        private bool hasMoreHistory = false;
        private List<StepHistoryResponseDto> historyInUI = new();
        private List<string> messagesInUI = new();


        [Inject]
        public StepService stepService { get; set; } = default!;

        [Inject]
        public StepHistoryService stepHistoryService { get; set; } = default!;

        [Inject]
        public UserService userService { get; set; } = default!;

        [Inject] 
        private NavigationManager Navigation { get; set; }

        [Parameter]
        public EventCallback<string> OnTabChange { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await LoadHistory();
            await LoadMessagesAsync();
        }

        private async Task GoBack()
        {
            if (OnTabChange.HasDelegate)
            {
                await OnTabChange.InvokeAsync("DEPARTMENTS");
            }
        }

        private async Task LoadHistory(int page = 1)
        {
            try
            {
                var payload = new QueriedStepHistoryRequestDto
                {
                    QueryParams = new QueryParams
                    {
                        Page = page,
                        PageSize = pageSize
                    }
                };

                var response = await stepHistoryService.GetStepHistoriesQueriedAsync(payload);

                if (response != null)
                {
                    var pageHistory = response.Result.Data
                        .OrderByDescending(h => h.DateTime)
                        .ToList();

                    historyInUI.AddRange(pageHistory);

                    foreach (var item in pageHistory)
                    {
                        messagesInUI.Add(await RenderMessageAsync(item));
                    }

                    currentPage++;
                    hasMoreHistory = pageHistory.Count == pageSize;

                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading step history: {ex.Message}");
            }
        }

        private async Task LoadMessagesAsync()
        {
            messages.Clear();
            foreach (var item in history)
            {
                messages.Add(await RenderMessageAsync(item));
            }
        }

        private async Task<string> RenderMessageAsync(StepHistoryResponseDto item)
        {
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(item.Details ?? "{}");
                var root = doc.RootElement;

                if (item.Action == "Change Name")
                {
                    return $"The department '{root.GetProperty("OldName").GetString()}' was changed into '{root.GetProperty("NewName").GetString()}'.";
                }
                else if (item.Action == "Delete Department")
                {
                    return $"The department '{root.GetProperty("DepartmentName").GetString()}' was deleted.";
                }
                else if (item.Action == "Create Department")
                {
                    return $"The department '{root.GetProperty("DepartmentName").GetString()}' was created.";
                }
                else if (item.Action == "Move Users")
                {
                    var users = root.GetProperty("Users")
                        .EnumerateArray()
                        .Select(u => u.GetString())
                        .Where(name => !string.IsNullOrEmpty(name))
                        .ToList();

                    return $"The users {string.Join(", ", users)} " +
                           $"from '{root.GetProperty("From").GetString()}' " +
                           $"were moved into '{root.GetProperty("To").GetString()}'.";
                }

                return item.Details ?? string.Empty;
            }
            catch
            {
                return item.Details ?? string.Empty;
            }
        }
    }
}
