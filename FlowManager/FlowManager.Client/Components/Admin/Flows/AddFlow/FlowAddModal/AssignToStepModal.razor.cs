using FlowManager.Client.DTOs;
using FlowManager.Client.Services;
using FlowManager.Client.ViewModels;
using FlowManager.Client.ViewModels.Team;
using FlowManager.Shared.DTOs.Requests.Team;
using FlowManager.Shared.DTOs.Requests.User;
using FlowManager.Shared.DTOs.Responses;
using FlowManager.Shared.DTOs.Responses.Team;
using FlowManager.Shared.DTOs.Responses.User;
using Microsoft.AspNetCore.Components;

namespace FlowManager.Client.Components.Admin.Flows.AddFlow.FlowAddModal
{
    public partial class AssignToStepModal : ComponentBase
    {
        [Parameter] public bool ShowAssignToStepModal { get; set; }
        [Parameter] public EventCallback<bool> ShowAssignToStepModalChanged { get; set; }
        [Parameter] public StepVM StepToAssign { get; set; }
        [Parameter] public EventCallback<StepVM> StepToAssignChanged { get; set; }
        [Parameter] public EventCallback OnStepAssigned { get; set; }

        [Inject] private TeamService _teamService { get; set; } = default!;
        [Inject] private UserService _userService { get; set; } = default!;

        private bool _isTeamsTabSelected = true;

        private List<Guid> _assignedModeratorIds = new List<Guid>();
        private List<Guid> _assignedTeamIds = new List<Guid>();

        private List<UserVM> _availableModerators = new List<UserVM>();
        private List<TeamVM> _availableTeams = new List<TeamVM>();

        private string _teamsSearchTerm = string.Empty;
        private string _usersSearchTerm = string.Empty;

        private int _teamsPageSize = 5;
        private int _teamsPage = 1;
        private int _teamsTotalCount = 0;
        private int _teamsTotalPages = 0;

        private int _usersPageSize = 10;
        private int _usersPage = 1;
        private int _usersTotalCount = 0;
        private int _usersTotalPages = 0;

        private string _onSubmitMessage = string.Empty;
        private bool _onSubmitSuccess = false;

        private Timer? _searchTimer;
        private const int SearchDelayMs = 300;

        protected override async Task OnInitializedAsync()
        {
            await LoadTeams();
            await LoadUsers();
        }

        private async Task SelectTab(bool isTeamsTab)
        {
            _isTeamsTabSelected = isTeamsTab;

            if (isTeamsTab)
            {
                _teamsSearchTerm = string.Empty;
                _teamsPage = 1;
                await LoadTeams();
            }
            else
            {
                _usersSearchTerm = string.Empty;
                _usersPage = 1;
                await LoadUsers();
            }

            StateHasChanged();
        }

        private void ToggleTeamSelection(Guid teamId, bool isSelected)
        {
            var team = _availableTeams.FirstOrDefault(t => t.Id == teamId);
            if (team == null) return;

            if (isSelected)
            {
                if (!_assignedTeamIds.Contains(teamId))
                {
                    _assignedTeamIds.Add(teamId);
                }

                foreach (var user in team.Users)
                {
                    _assignedModeratorIds.Remove(user.Id);
                }
            }
            else
            {
                _assignedTeamIds.Remove(teamId);
            }

            _onSubmitMessage = string.Empty;
            StateHasChanged();
        }

        private void ToggleUserSelectionFromTeam(TeamVM team, UserVM user, bool isSelected)
        {
            if (isSelected)
            {
                if (!_assignedModeratorIds.Contains(user.Id))
                {
                    _assignedModeratorIds.Add(user.Id);
                }

                var allTeamUsersSelected = team.Users.All(u => _assignedModeratorIds.Contains(u.Id));

                if (allTeamUsersSelected)
                {
                    if (!_assignedTeamIds.Contains(team.Id))
                    {
                        _assignedTeamIds.Add(team.Id);
                    }

                    foreach (var teamUser in team.Users)
                    {
                        _assignedModeratorIds.Remove(teamUser.Id);
                    }
                }
                else
                {
                    _assignedTeamIds.Remove(team.Id);
                }
            }
            else
            {
                _assignedModeratorIds.Remove(user.Id);

                if (_assignedTeamIds.Contains(team.Id))
                {
                    _assignedTeamIds.Remove(team.Id);

                    foreach (var teamUser in team.Users)
                    {
                        if (teamUser.Id != user.Id && !_assignedModeratorIds.Contains(teamUser.Id))
                        {
                            _assignedModeratorIds.Add(teamUser.Id);
                        }
                    }
                }
            }

            _onSubmitMessage = string.Empty;
            StateHasChanged();
        }

        private bool IsTeamFullySelected(TeamVM team)
        {
            return _assignedTeamIds.Contains(team.Id) ||
                   team.Users.All(u => _assignedModeratorIds.Contains(u.Id));
        }

        private bool IsUserFromTeamSelected(TeamVM team, UserVM user)
        {
            return _assignedTeamIds.Contains(team.Id) || _assignedModeratorIds.Contains(user.Id);
        }

        private void ToggleUserSelection(Guid userId, bool isSelected)
        {
            if (isSelected)
            {
                if (!_assignedModeratorIds.Contains(userId))
                {
                    _assignedModeratorIds.Add(userId);
                }
            }
            else
            {
                _assignedModeratorIds.Remove(userId);
            }

            _onSubmitMessage = string.Empty;
            StateHasChanged();
        }

        private void OnTeamsSearchChanged()
        {
            _searchTimer?.Dispose();
            _searchTimer = new Timer(async _ =>
            {
                _teamsPage = 1;
                await InvokeAsync(async () =>
                {
                    await LoadTeams();
                    StateHasChanged();
                });
            }, null, SearchDelayMs, Timeout.Infinite);
        }

        private void OnUsersSearchChanged()
        {
            _searchTimer?.Dispose();
            _searchTimer = new Timer(async _ =>
            {
                _usersPage = 1;
                await InvokeAsync(async () =>
                {
                    await LoadUsers();
                    StateHasChanged();
                });
            }, null, SearchDelayMs, Timeout.Infinite);
        }

        private async Task LoadTeams()
        {
            try
            {
                QueriedTeamRequestDto payload = new QueriedTeamRequestDto();

                if (!string.IsNullOrEmpty(_teamsSearchTerm))
                {
                    payload.GlobalSearchTerm = _teamsSearchTerm;
                }

                if (_teamsPageSize != 0 && _teamsPage != 0)
                {
                    payload.QueryParams = new Shared.DTOs.Requests.QueryParamsDto
                    {
                        Page = _teamsPage,
                        PageSize = _teamsPageSize,
                    };
                }

                ApiResponse<PagedResponseDto<TeamResponseDto>> response =
                    await _teamService.GetAllModeratorTeamsByStepIdAsync(StepToAssign.Id, payload);

                if (!response.Success)
                {
                    _availableTeams = new List<TeamVM>();
                    _teamsTotalCount = 0;
                    _teamsTotalPages = 0;
                    return;
                }

                _availableTeams = response.Result.Data.Select(t => new TeamVM
                {
                    Id = t.Id,
                    Name = t.Name,
                    Users = t.Users!.Select(u => new UserVM
                    {
                        Id = u.Id,
                        Name = u.Name,
                        Email = u.Email,
                    }).ToList()
                }).ToList();

                _teamsTotalCount = response.Result.TotalCount;
                _teamsTotalPages = response.Result.TotalPages;
            }
            catch (Exception ex)
            {
                _onSubmitMessage = $"Error loading teams: {ex.Message}";
                _onSubmitSuccess = false;
            }
        }

        private async Task LoadUsers()
        {
            try
            {
                QueriedUserRequestDto payload = new QueriedUserRequestDto();

                if (!string.IsNullOrEmpty(_usersSearchTerm))
                {
                    payload.GlobalSearchTerm = _usersSearchTerm;
                }

                if (_usersPageSize != 0 && _usersPage != 0)
                {
                    payload.QueryParams = new Shared.DTOs.Requests.QueryParamsDto
                    {
                        Page = _usersPage,
                        PageSize = _usersPageSize
                    };
                }

                ApiResponse<PagedResponseDto<UserResponseDto>> response =
                    await _userService.GetUnassignedModeratorsByStepIdQueriedAsync(StepToAssign.Id, payload);

                if (!response.Success)
                {
                    _availableModerators = new List<UserVM>();
                    _usersTotalCount = 0;
                    _usersTotalPages = 0;
                    return;
                }

                _availableModerators = response.Result.Data.Select(u => new UserVM
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                }).ToList();

                _usersTotalCount = response.Result.TotalCount;
                _usersTotalPages = response.Result.TotalPages;

                Console.WriteLine();
            }
            catch (Exception ex)
            {
                _onSubmitMessage = $"Error loading users: {ex.Message}";
                _onSubmitSuccess = false;
            }
        }

        private async Task PreviousTeamsPage()
        {
            if (_teamsPage > 1)
            {
                _teamsPage--;
                await LoadTeams();
            }
        }

        private async Task NextTeamsPage()
        {
            if (_teamsPage < _teamsTotalPages)
            {
                _teamsPage++;
                await LoadTeams();
            }
        }

        private async Task PreviousUsersPage()
        {
            if (_usersPage > 1)
            {
                _usersPage--;
                await LoadUsers();
            }
        }

        private async Task NextUsersPage()
        {
            if (_usersPage < _usersTotalPages)
            {
                _usersPage++;
                await LoadUsers();
            }
        }

        private async Task SubmitAssignment()
        {
            try
            {
                if (!_assignedTeamIds.Any() && !_assignedModeratorIds.Any())
                {
                    _onSubmitMessage = "Please select at least one team or user to assign.";
                    _onSubmitSuccess = false;
                    return;
                }

                var updatedStep = new StepVM
                {
                    Id = StepToAssign.Id,
                    Name = StepToAssign.Name,
                    Teams = _assignedTeamIds.Select(tId => new TeamVM { Id = tId }).ToList() ,
                    Users = _assignedModeratorIds.Select(uId => new UserVM { Id = uId }).ToList(),
                };

                await StepToAssignChanged.InvokeAsync(updatedStep);

                _onSubmitMessage = "Assignment completed successfully!";
                _onSubmitSuccess = true;

                await Task.Delay(1500);
                
                await OnStepAssigned.InvokeAsync();

                await CancelForm();
            }
            catch (Exception ex)
            {
                _onSubmitMessage = $"Error during assignment: {ex.Message}";
                _onSubmitSuccess = false;
            }
        }

        private async Task CancelForm()
        {
            _assignedModeratorIds.Clear();
            _assignedTeamIds.Clear();
            _onSubmitMessage = string.Empty;
            _onSubmitSuccess = false;
            _isTeamsTabSelected = true;
            _teamsSearchTerm = string.Empty;
            _usersSearchTerm = string.Empty;
            _teamsPage = 1;
            _usersPage = 1;

            await ShowAssignToStepModalChanged.InvokeAsync();
        }
    }
}
