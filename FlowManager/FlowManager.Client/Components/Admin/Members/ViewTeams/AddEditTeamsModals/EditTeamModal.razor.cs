using FlowManager.Client.DTOs;
using FlowManager.Client.Services;
using FlowManager.Client.ViewModels;
using FlowManager.Client.ViewModels.Team;
using FlowManager.Shared.DTOs.Requests;
using FlowManager.Shared.DTOs.Requests.Team;
using FlowManager.Shared.DTOs.Responses.Team;
using Microsoft.AspNetCore.Components;

namespace FlowManager.Client.Components.Admin.Members.ViewTeams.AddEditTeamsModals
{
    public partial class EditTeamModal : ComponentBase
    {
        [Parameter] public bool ShowEditTeamModal { get; set; }
        [Parameter] public EventCallback<bool> ShowEditTeamModalChanged { get; set; }
        [Parameter] public TeamVM TeamToEdit { get; set; }
        [Parameter] public EventCallback TeamWasEdited { get; set; }
        [Parameter] public StepVM TeamStep { get; set; }

        [Inject] private TeamService _teamService { get; set; } = default!;

        private string _searchTerm { get; set; } = string.Empty;

        private string _teamName { get; set; } = string.Empty;

        private bool _isSubmitting { get; set; } = false;
        private string _submitMessage { get; set; } = string.Empty;
        private bool _submitStatus { get; set; } = false;

        private HashSet<UserVM> _selectedUsers = new HashSet<UserVM>();
        private List<UserVM> _users = new List<UserVM>();

        private int _currentPage = 1;
        private int _pageSize = 10;
        private int _totalPages = 0;
        private int _totalCount = 0;

        protected override async Task OnInitializedAsync()
        {
            await LoadAssignedUsersInit();
            _teamName = TeamToEdit.Name!;
        }

        private async Task LoadAssignedUsersInit()
        {
            QueriedTeamRequestDto payload = new QueriedTeamRequestDto();
            if (!string.IsNullOrEmpty(_searchTerm))
            {
                payload.GlobalSearchTerm = _searchTerm;
            }

            payload.QueryParams = new QueryParamsDto
            {
                Page = _currentPage,
                PageSize = _pageSize
            };

            ApiResponse<SplitUsersByTeamIdResponseDto> response = await _teamService.GetSplitUsersByTeamIdAsync(TeamStep.Id ?? Guid.Empty, TeamToEdit.Id, payload);

            _selectedUsers = response.Result.AssignedToTeamUsers.Select(u => new UserVM
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
            }).ToHashSet();

            _users = response.Result.AssignedToTeamUsers.Concat(response.Result.UnassignedToTeamUsers).Select(u => new UserVM
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
            }).ToList();

            _totalPages = response.Result.TotalPages;
            _totalCount = response.Result.TotalCountUnassigned + response.Result.TotalCountAssigned;

            StateHasChanged();
        }

        private bool IsSubmitValid()
        {
            return _isSubmitting == false;
        }

        private async Task LoadUsersAsync(bool resetPageSize = false)
        {
            QueriedTeamRequestDto payload = new QueriedTeamRequestDto();
            if (!string.IsNullOrEmpty(_searchTerm))
            {
                payload.GlobalSearchTerm = _searchTerm;
            }

            if (resetPageSize)
            {
                _pageSize = 10;
                payload.QueryParams = new QueryParamsDto
                {
                    Page = 1,
                    PageSize = _pageSize
                };
            }
            else if (_currentPage != 0 && _pageSize != 0)
            {
                payload.QueryParams = new QueryParamsDto
                {
                    Page = _currentPage,
                    PageSize = _pageSize
                };
            }

            ApiResponse<SplitUsersByTeamIdResponseDto> response = await _teamService.GetSplitUsersByTeamIdAsync(TeamStep.Id ?? Guid.Empty, TeamToEdit.Id, payload);

            if (!response.Success)
            {
                Console.WriteLine("No users found");
                _users = new List<UserVM>();
                _totalPages = 0;
                _totalCount = 0;
                return;
            }

            _users = response.Result.AssignedToTeamUsers.Concat(response.Result.UnassignedToTeamUsers).Select(u => new UserVM
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
            }).ToList();

            _totalPages = response.Result.TotalPages;
            _totalCount = response.Result.TotalCountUnassigned + response.Result.TotalCountAssigned;
            Console.WriteLine($"Total pages: {_totalPages}");
            Console.WriteLine($"Total count: {_totalCount}");
        }

        private async Task OnCancel()
        {
            _searchTerm = string.Empty;
            _teamName = string.Empty;

            await ShowEditTeamModalChanged.InvokeAsync(false);
        }

        private async Task OnSubmit()
        {
            await EditTeamAsync();

            await Task.Delay(3000);

            _submitMessage = string.Empty;

            await LoadUsersAsync();
        }

        private async Task EditTeamAsync()
        {
            _isSubmitting = true;

            PatchTeamRequestDto payload = new PatchTeamRequestDto();

            if(!string.IsNullOrEmpty(_teamName))
            {
                payload.Name = _teamName;
            }

            if(_selectedUsers != null && _selectedUsers.Count > 0)
            {
                payload.UserIds = _selectedUsers.Select(u => u.Id).ToList();
            }

            ApiResponse<TeamResponseDto> result = await _teamService.PatchTeamAsync(TeamToEdit.Id, payload);

            _isSubmitting = false;

            _submitStatus = result.Success;
            _submitMessage = result.Message;
            StateHasChanged();

            if (_submitStatus)
            {
                await TeamWasEdited.InvokeAsync();
            }
        }

        private bool IsUserAssigned(UserVM user)
        {
            return _selectedUsers.Contains(user);
        }

        private void ToggleUserSelection(Guid userId, bool isSelected)
        {
            UserVM? user = _users.FirstOrDefault(u => u.Id == userId);

            if (isSelected)
            {
                _selectedUsers.Add(user);
            }
            else
            {
                _selectedUsers.Remove(user);
            }

            StateHasChanged();
        }

        private void RemoveUserFromAssignment(Guid userId)
        {
            var userToRemove = _selectedUsers.FirstOrDefault(u => u.Id == userId);
            if (userToRemove != null)
            {
                _selectedUsers.Remove(userToRemove);
                StateHasChanged();
            }
        }

        private async Task LoadMore()
        {
            _pageSize += 10;
            await LoadUsersAsync();
        }
    }
}
