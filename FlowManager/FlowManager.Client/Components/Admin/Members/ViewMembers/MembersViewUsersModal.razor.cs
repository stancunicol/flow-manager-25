using FlowManager.Client.DTOs;
using FlowManager.Client.Services;
using FlowManager.Client.ViewModels;
using FlowManager.Shared.DTOs.Requests.User;
using FlowManager.Shared.DTOs.Responses;
using FlowManager.Shared.DTOs.Responses.User;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace FlowManager.Client.Components.Admin.Members.ViewMembers
{
    public partial class MembersViewUsersModal : ComponentBase, IDisposable
    {
        [Inject] private UserService _userService { get; set; } = default!;
        [Inject] private ILogger<Members> _logger { get; set; } = default!;

        private bool _showAddForm = false;
        private bool _showEditForm = false;

        private List<UserVM> _users = new();

        private UserVM _selectedUserToEdit = new();

        private string _searchTerm = string.Empty;

        private int _pageSize = 10;
        private int _currentPage = 1;
        private int _totalPages = 0;
        private int _maxVisiblePages = 4;
        private int _totalCount = 0;

        private System.Threading.Timer? _debounceTimer;
        private readonly int _debounceDelayMs = 250;

        protected override async Task OnInitializedAsync()
        {
            await LoadUsers();
        }

        private void OnSearchTermChanged(string newSearchTerm)
        {
            _searchTerm = newSearchTerm;

            _debounceTimer?.Dispose();

            _debounceTimer = new System.Threading.Timer(async _ =>
            {
                await InvokeAsync(async () =>
                {
                    _searchTerm = _searchTerm.Trim();
                    _currentPage = 1; 
                    await LoadUsers();
                    StateHasChanged();
                });

                _debounceTimer?.Dispose();
                _debounceTimer = null;

            }, null, _debounceDelayMs, Timeout.Infinite);
        }

        private async Task LoadUsers()
        {
            QueriedUserRequestDto payload = new QueriedUserRequestDto();

            if (!string.IsNullOrEmpty(_searchTerm))
            {
                payload.GlobalSearchTerm = _searchTerm;
            }

            if (_currentPage != 0 && _pageSize != 0)
            {
                payload.QueryParams = new Shared.DTOs.Requests.QueryParamsDto
                {
                    Page = _currentPage,
                    PageSize = _pageSize,
                };
            }

            ApiResponse<PagedResponseDto<UserResponseDto>> response = await _userService.GetAllUsersQueriedAsync(payload);

            if (!response.Success)
            {
                _users = new();
                return;
            }

            _users = response.Result.Data.Select(u => new UserVM
            {
                Id = u.Id,
                Name = u.Name!,
                Email = u.Email!,
                PhoneNumber = u.PhoneNumber,
                IsActive = u.DeletedAt == null,
                Roles = u.Roles!.Select(r => new RoleVM
                {
                    Id = r.Id,
                    RoleName = r.Name!
                }).ToList(),
                Step = new StepVM
                {
                    Id = u.Step?.StepId ?? Guid.Empty,
                    Name = u.Step?.StepName ?? string.Empty
                }
            }).ToList();

            _totalPages = response.Result.TotalPages;
            _totalCount = response.Result.TotalCount;
        }

        private async Task DeleteUser(UserVM user)
        {
            ApiResponse<UserResponseDto> response = await _userService.DeleteUserAsync(user.Id);

            if (!response.Success)
            {
                _logger.LogError("Failed to delete user {UserId}: {Message}", user.Id, response.Message);
            }

            await LoadUsers();
        }

        private async Task RestoreUser(UserVM user)
        {
            ApiResponse<UserResponseDto> response = await _userService.RestoreUserAsync(user.Id);

            if (!response.Success)
            {
                _logger.LogError("Failed to restore user {UserId}: {Message}", user.Id, response.Message);
            }

            await LoadUsers();
        }

        private void EditUser(UserVM user)
        {
            _selectedUserToEdit = user;
            _showEditForm = true;
        }

        private async Task OnEnterPressed(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
            {
                _debounceTimer?.Dispose();
                _debounceTimer = null;

                _searchTerm = _searchTerm.Trim();
                _currentPage = 1;
                await LoadUsers();
            }
        }

        private async Task SearchFlows()
        {
            _debounceTimer?.Dispose();
            _debounceTimer = null;

            _searchTerm = _searchTerm.Trim();
            _currentPage = 1;
            await LoadUsers();
        }

        private async Task GoToFirstPage()
        {
            _currentPage = 1;
            await LoadUsers();
        }

        private async Task GoToPreviousPage()
        {
            _currentPage--;
            await LoadUsers();
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
            await LoadUsers();
        }

        private async Task GoToNextPage()
        {
            _currentPage++;
            await LoadUsers();
        }

        private async Task GoToLastPage()
        {
            _currentPage = _totalPages;
            await LoadUsers();
        }

        public void Dispose()
        {
            _debounceTimer?.Dispose();
        }
    }
}