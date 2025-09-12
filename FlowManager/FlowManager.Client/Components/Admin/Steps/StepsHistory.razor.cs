using FlowManager.Shared.DTOs.Responses.StepHistory;
using Microsoft.AspNetCore.Components;
using FlowManager.Client.Services;

namespace FlowManager.Client.Components.Admin.Steps
{
    public partial class StepsHistory: ComponentBase
    {
        List<StepHistoryResponseDto> history = new();
        private List<string> messages = new();

        [Inject]
        public StepService stepService { get; set; } = default!;

        [Inject]
        public StepHistoryService stepHistoryService { get; set; } = default!;
        [Inject]
        public UserService userService { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            await LoadHistory();
            await LoadMessagesAsync();
        }

        public async Task LoadHistory()
        {
            try
            {
                var response = await stepHistoryService.GetAllAsync();

                if (response != null)
                    history = response.ToList();

                Console.WriteLine($"Loaded {response?.Count() ?? 0} history items");
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
