using FlowManager.Client.DTOs;
using FlowManager.Client.Services;
using FlowManager.Client.ViewModels;
using FlowManager.Shared.DTOs.Requests;
using FlowManager.Shared.DTOs.Requests.Team;
using FlowManager.Shared.DTOs.Requests.User;
using FlowManager.Shared.DTOs.Responses;
using FlowManager.Shared.DTOs.Responses.Step;
using FlowManager.Shared.DTOs.Responses.Team;
using FlowManager.Shared.DTOs.Responses.User;
using Microsoft.AspNetCore.Components;
using System.Collections;
using System.Threading.Tasks;

namespace FlowManager.Client.Components.Admin.Members.ViewTeams.AddEditTeamsModals
{
    public partial class AddTeamModal : ComponentBase
    {
        [Parameter] public bool ShowAddTeamModal { get; set; }
        [Parameter] public EventCallback<bool> ShowAddTeamModalChanged { get; set; }
        [Parameter] public EventCallback TeamWasAdded { get; set; }

        [Inject] private UserService _userService { get; set; } = default!;
        [Inject] private TeamService _teamService { get;set; } = default!;
        [Inject] private StepService _stepService { get; set; } = default!;

        private string _searchTerm { get; set; } = string.Empty;

        private string _teamName { get; set; } = string.Empty; 

        private bool _isSubmitting { get; set; } = false;
        private string _submitMessage { get; set; } = string.Empty;
        private bool _submitStatus { get; set; } = false;

        private bool _isDropdownOpen = false;
        private string _selectedStepName = string.Empty;
        private Guid _selectedStepId = Guid.Empty;

        private List<UserVM> _users = new List<UserVM>();
        private List<StepVM> _availableSteps = new List<StepVM>();
        private HashSet<UserVM> _selectedUsers = new HashSet<UserVM>();

        private int _currentPage = 1;
        private int _pageSize = 10;
        private int _totalPages = 0;
        private int _totalCount = 0;

        protected override async Task OnInitializedAsync()
        {
            await LoadUsersAsync();
            await LoadStepsAsync();
        }

        private async Task LoadStepsAsync()
        {
            ApiResponse<PagedResponseDto<StepResponseDto>> response = await _stepService.GetAllStepsIncludeUsersAndTeamsQueriedAsync();

            if (!response.Success)
            {
                _availableSteps = new();
            }

            _availableSteps = response.Result.Data.Select(s => new StepVM
            {
                Id = s.StepId,
                Name = s.StepName
            }).ToList();
        }

        private async Task LoadUsersAsync(bool resetPageSize = false)
        {
            QueriedUserRequestDto payload = new QueriedUserRequestDto();
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

            ApiResponse<PagedResponseDto<UserResponseDto>> response = await _userService.GetAllUsersByStepQueriedAsync(_selectedStepId, payload);

            if (!response.Success || response.Result?.Data == null)
            {
                Console.WriteLine("No users found or request failed");
                _users = new List<UserVM>();
                _totalPages = 0;
                _totalCount = 0;
                return;
            }

            _users = response.Result.Data.Select(u => new UserVM
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email
            }).ToList();

            _totalPages = response.Result.TotalPages;
            _totalCount = response.Result.TotalCount;
            Console.WriteLine($"Total pages: {_totalPages}");
            Console.WriteLine($"Total count: {_totalCount}");
        }

        private async Task OnCancel()
        {
            _searchTerm = string.Empty;
            _teamName = string.Empty;

            await ShowAddTeamModalChanged.InvokeAsync(false);
        }

        private async Task OnSubmit()
        {
            await PostTeamAsync();

            await Task.Delay(3000);
            _submitMessage = string.Empty;

            ClearForm();

            await LoadUsersAsync();
        }

        private async Task PostTeamAsync()
        {
            _isSubmitting = true;
            PostTeamRequestDto payload = new PostTeamRequestDto
            {
                Name = _teamName,
                UserIds = _selectedUsers.Select(u => u.Id).ToList()
            };

            ApiResponse<TeamResponseDto> result = await _teamService.PostTeamAsync(payload);

            _isSubmitting = false;

            Console.WriteLine($"message {result.Message}");
            _submitStatus = result.Success;
            _submitMessage = result.Message;
            StateHasChanged();

            if (_submitStatus)
            {
                await TeamWasAdded.InvokeAsync();
            }
        }

        private void ClearForm()
        {
            _searchTerm = string.Empty;
            _teamName = string.Empty;
            _isSubmitting = false;
        }

        private async Task LoadMore()
        {
            _pageSize += 10;
            await LoadUsersAsync();
        }

        private List<UserVM> GetAssignedUsers()
        {
            return _selectedUsers.ToList();
        }

        private int GetAssignedUsersCount()
        {
            if (_selectedUsers == null || _selectedUsers.Count == 0)
                return 0;

            return _selectedUsers.Count;
        }

        private void RemoveUserFromAssignment(Guid userId)
        {
            UserVM? userToDelete = _selectedUsers.FirstOrDefault(u => u.Id == userId);

            _selectedUsers.Remove(userToDelete);
            StateHasChanged();
        }

        private bool IsSubmitValid()
        {
            return !string.IsNullOrEmpty(_teamName) && _selectedUsers.Count > 0 && _selectedStepId != Guid.Empty;
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

        private void ToggleDropdown()
        {
            _isDropdownOpen = !_isDropdownOpen;
        }

        private async Task SelectStep(StepVM step)
        {
            if (_selectedStepId != step.Id)
            {
                _selectedStepId = step.Id ?? Guid.Empty;
                _selectedStepName = step.Name ?? string.Empty;
                _selectedUsers.Clear();
                await LoadUsersAsync(resetPageSize: true);
            }
            _isDropdownOpen = false;
        }

        private bool IsSearchInvalid()
        {
            return _selectedStepId == Guid.Empty;
        }
    }
}
