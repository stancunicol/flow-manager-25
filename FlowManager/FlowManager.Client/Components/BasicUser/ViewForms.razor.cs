using FlowManager.Client.Services;
using FlowManager.Shared.DTOs.Requests.FormResponse;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace FlowManager.Client.Components.BasicUser
{
    public partial class ViewForms : ComponentBase, IDisposable
    {
        // Status filter state
        private HashSet<string> selectedStatuses = new HashSet<string> { "Pending", "Rejected", "Approved" };

        // User forms state with search
        private bool isLoadingUserForms = false;
        private List<FormResponseResponseDto>? userForms;
        private List<FormResponseResponseDto>? filteredUserForms;
        private string userFormsSearchTerm = "";
        private Timer? userFormsSearchDebounceTimer;
        private Guid currentUserId = Guid.Empty;

        // Pagination state for user forms
        private int _pageSize = 8;
        private int _currentPage = 1;
        private int _totalPages = 0;
        private int _maxVisiblePages = 4;
        private int _totalCount = 0;

        [Inject] protected FormResponseService FormResponseService { get; set; } = default!;
        [Inject] protected NavigationManager Navigation { get; set; } = default!;
        [Inject] protected AuthenticationStateProvider AuthProvider { get; set; } = default!;
        [Inject] protected HttpClient Http { get; set; } = default!;
        [Inject] protected IJSRuntime JSRuntime { get; set; } = default!;

        [Parameter] public EventCallback<FormResponseResponseDto> OnViewFormResponse { get; set; }
        [Parameter] public EventCallback OnAddForm { get; set; }
        [Parameter] public EventCallback OnCompleteOnBehalf { get; set; }
        [Parameter] public Guid UserId { get; set; }

        protected override async Task OnInitializedAsync()
        {
            currentUserId = UserId;
            if (currentUserId != Guid.Empty)
            {
                await LoadUserForms();
            }
        }

        protected override async Task OnParametersSetAsync()
        {
            if (UserId != currentUserId)
            {
                currentUserId = UserId;
                if (currentUserId != Guid.Empty)
                {
                    await LoadUserForms();
                }
            }
        }

        private async Task LoadUserForms()
        {
            isLoadingUserForms = true;
            StateHasChanged();

            try
            {
                var response = await FormResponseService.GetFormResponsesByUserPagedAsync(
                    currentUserId,
                    _currentPage,
                    _pageSize,
                    userFormsSearchTerm,
                    selectedStatuses.ToList()
                );

                if (response != null)
                {
                    userForms = response.FormResponses;
                    _totalCount = response.TotalCount;
                    _totalPages = (int)Math.Ceiling((double)response.TotalCount / _pageSize);
                }
                else
                {
                    userForms = new List<FormResponseResponseDto>();
                    _totalCount = 0;
                    _totalPages = 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading user forms: {ex.Message}");
                userForms = new List<FormResponseResponseDto>();
                _totalCount = 0;
                _totalPages = 0;
            }
            finally
            {
                isLoadingUserForms = false;
                StateHasChanged();
            }
        }

        private async Task FilterByStatus(string status)
        {
            selectedStatuses = new HashSet<string> { status };
            _currentPage = 1;
            await LoadUserForms();
        }

        private async Task ShowAllStatuses()
        {
            selectedStatuses = new HashSet<string> { "Pending", "Rejected", "Approved" };
            _currentPage = 1;
            await LoadUserForms();
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

        // USER FORMS SEARCH FUNCTIONALITY
        private void OnUserFormsSearchInput(ChangeEventArgs e)
        {
            userFormsSearchTerm = e.Value?.ToString() ?? "";

            userFormsSearchDebounceTimer?.Dispose();
            userFormsSearchDebounceTimer = new Timer(async _ =>
            {
                _currentPage = 1;
                await InvokeAsync(async () =>
                {
                    await LoadUserForms();
                    StateHasChanged();
                });
            }, null, 500, Timeout.Infinite);
        }

        private async Task ClearUserFormsSearch()
        {
            userFormsSearchTerm = "";
            _currentPage = 1;
            await LoadUserForms();
        }

        private async Task GoToFirstPage()
        {
            _currentPage = 1;
            await LoadUserForms();
        }

        private async Task GoToPreviousPage()
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                await LoadUserForms();
            }
        }

        private List<int> GetPageNumbers()
        {
            var startPage = Math.Max(1, _currentPage - (_maxVisiblePages / 2));
            var endPage = Math.Min(_totalPages, startPage + _maxVisiblePages - 1);

            if (endPage - startPage + 1 < _maxVisiblePages)
            {
                startPage = Math.Max(1, endPage - _maxVisiblePages + 1);
            }

            return Enumerable.Range(startPage, endPage - startPage + 1).ToList();
        }

        private async Task GoToPage(int page)
        {
            _currentPage = page;
            await LoadUserForms();
        }

        private async Task GoToNextPage()
        {
            if (_currentPage < _totalPages)
            {
                _currentPage++;
                await LoadUserForms();
            }
        }

        private async Task GoToLastPage()
        {
            _currentPage = _totalPages;
            await LoadUserForms();
        }

        private async Task ViewFormResponse(FormResponseResponseDto formResponse)
        {
            await OnViewFormResponse.InvokeAsync(formResponse);
        }

        private async Task RefreshUserForms()
        {
            await LoadUserForms();
        }

        public void Dispose()
        {
            userFormsSearchDebounceTimer?.Dispose();
        }
    }
}
