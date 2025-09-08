using FlowManager.Client.Services;
using FlowManager.Shared.DTOs.Requests;
using FlowManager.Shared.DTOs.Requests.Flow;
using FlowManager.Shared.DTOs.Responses.Flow;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace FlowManager.Client.Components.Admin.Flows.AddFlow
{
    public partial class FlowsViewModal : ComponentBase, IDisposable
    {
        [Parameter] public EventCallback<FlowResponseDto> OnEditFlowRequested { get; set; }
        [Inject] private FlowService _flowService { get; set; } = default!;

        private List<FlowResponseDto> _flows = new();
        private bool _isLoading = false;
        private string _searchTerm = string.Empty;
        private bool _showEditModal = false;
        private FlowResponseDto? _selectedFlow = null;

        private System.Threading.Timer? _debounceTimer;
        private int _debounceDelayMs = 250;

        private int _pageSize = 9;
        private int _currentPage = 1;
        private int _totalPages = 1;
        private int _totalCount = 1;
        private const int _maxVisiblePages = 4;

        protected override async Task OnInitializedAsync()
        {
            await LoadFlows();
        }

        private async Task LoadFlows()
        {
            _isLoading = true;
            StateHasChanged();

            try
            {
                Console.WriteLine($"term : {_searchTerm}");
                var payload = new QueriedFlowRequestDto
                {
                    GlobalSearchTerm = _searchTerm,
                    QueryParams = new QueryParamsDto
                    {
                        Page = _currentPage,
                        PageSize = _pageSize
                    }
                };

                var response = await _flowService.GetAllFlowsQueriedAsync(payload);

                if (response.Success && response.Result != null)
                {
                    _flows = response.Result.Data.ToList();
                }
                else
                {
                    _flows = new List<FlowResponseDto>();
                }

                Console.WriteLine($"Flows loaded: {_flows.Count} items.");
            }
            catch (Exception ex)
            {
                _flows = new List<FlowResponseDto>();
            }
            finally
            {
                _isLoading = false;
                StateHasChanged();
            }
        }

        public async Task RefreshFlows()
        {
            await LoadFlows();
        }

        private void OnSearchInput(ChangeEventArgs e)
        {
            var newSearchTerm = e.Value?.ToString() ?? "";

            _debounceTimer?.Dispose();
            _debounceTimer = new Timer(async _ =>
            {
                _searchTerm = newSearchTerm;
                await InvokeAsync(async () =>
                {
                    await LoadFlows();
                });
            }, null, _debounceDelayMs, Timeout.Infinite);
        }

        private async Task OnEnterPressed(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
            {
                _debounceTimer?.Dispose();
                await LoadFlows();
            }
        }

        private void OpenEditModal(FlowResponseDto flow)
        {
            if (OnEditFlowRequested.HasDelegate)
            {
                OnEditFlowRequested.InvokeAsync(flow);
                StateHasChanged();
            }
            else
            {
                // Altfel funcționăm local (pentru Flows.razor)
                _selectedFlow = flow;
                _showEditModal = true;
                StateHasChanged();
            }
        }

        private async Task CloseEditModal()
        {
            _showEditModal = false;
            _selectedFlow = null;
            StateHasChanged();
        }

        private async Task OnFlowUpdated(FlowResponseDto updatedFlow)
        {
            var index = _flows.FindIndex(f => f.Id == updatedFlow.Id);
            if (index >= 0)
            {
                _flows[index] = updatedFlow;
            }

            await CloseEditModal();
            await LoadFlows();
        }

        private async Task GoToFirstPage()
        {
            _currentPage = 1;
            await LoadFlows();
        }

        private async Task GoToPreviousPage()
        {
            _currentPage--;
            await LoadFlows();
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
            await LoadFlows();
        }

        private async Task GoToNextPage()
        {
            _currentPage++;
            await LoadFlows();
        }

        private async Task GoToLastPage()
        {
            _currentPage = _totalPages;
            await LoadFlows();
        }

        public void Dispose()
        {
            _debounceTimer?.Dispose();
        }
    }
}