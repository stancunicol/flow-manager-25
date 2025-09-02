using FlowManager.Client.DTOs;
using FlowManager.Client.Services;
using FlowManager.Client.ViewModels;
using FlowManager.Client.ViewModels.Team;
using FlowManager.Domain.Entities;
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

        private List<UserVM> _assignedModerators = new List<UserVM>();
        private List<TeamVM> _assignedTeams = new List<TeamVM>();

        private List<UserVM> _availableModerators = new List<UserVM>();
        private List<SelectTeamVM> _availableTeams = new List<SelectTeamVM>();

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
            await LoadTeamsAsync();
            await LoadUsersAsync();

            AssignTeamsFromExistingStep();
            AssignUsersFromExistingStep();
        }

        private void AssignTeamsFromExistingStep()
        {
            if (StepToAssign.Teams == null || StepToAssign.Teams.Count == 0)
                return;

            foreach(var team in StepToAssign.Teams)
            {
                _assignedTeams.Add(team);
            }
        }

        private void AssignUsersFromExistingStep()
        {
            if (StepToAssign.Users == null || StepToAssign.Users.Count == 0)
                return;

            foreach (var user in StepToAssign.Users)
            {
                _assignedModerators.Add(_availableModerators.First(u => u.Id == user.Id));
            }
        }

        private async Task SelectTab(bool isTeamsTab)
        {
            _isTeamsTabSelected = isTeamsTab;

            if (isTeamsTab)
            {
                _teamsSearchTerm = string.Empty;
                _teamsPage = 1;
                await LoadTeamsAsync();
            }
            else
            {
                _usersSearchTerm = string.Empty;
                _usersPage = 1;
                await LoadUsersAsync();
            }

            StateHasChanged();
        }

        private void ToggleTeamSelection(TeamVM team, bool isSelected)
        {
            if (isSelected)
            {
                _assignedTeams.RemoveAll(ast => ast.Id == team.Id);

                TeamVM availableTeam = _availableTeams.First(ast => ast.Team.Id == team.Id).Team;
                var assignedTeam = new TeamVM
                {
                    Id = availableTeam.Id,
                    Name = availableTeam.Name,
                    Users = new List<UserVM>(availableTeam.Users)
                };
                _assignedTeams.Add(assignedTeam);
            }
            else
            {
                _assignedTeams.RemoveAll(ast => ast.Id == team.Id);
            }

            _onSubmitMessage = string.Empty;
            StateHasChanged();
        }

        private void ToggleUserSelectionFromTeam(TeamVM team, UserVM user, bool isSelected)
        {
            if (isSelected)
            {
                TeamVM assignedTeam = _assignedTeams.FirstOrDefault(ast => ast.Id == team.Id);

                if (assignedTeam == null) // team not selected
                {
                    var newAssignedTeam = new TeamVM
                    {
                        Id = team.Id,
                        Name = team.Name,
                        Users = new List<UserVM> { user }
                    };
                    _assignedTeams.Add(newAssignedTeam);
                }
                else // team selected but user not assigned
                {
                    if (!assignedTeam.Users.Any(u => u.Id == user.Id))
                    {
                        assignedTeam.Users.Add(user);
                    }
                }
            }
            else
            {
                TeamVM assignedTeam = _assignedTeams.FirstOrDefault(ast => ast.Id == team.Id);
                if (assignedTeam != null)
                {
                    assignedTeam.Users.RemoveAll(u => u.Id == user.Id);

                    if (!assignedTeam.Users.Any())
                    {
                        _assignedTeams.Remove(assignedTeam);
                    }
                }
            }

            _onSubmitMessage = string.Empty;
            StateHasChanged();
        }

        private void OpenDropdownForTeam(TeamVM team)
        {
            var teamToSelect = _availableTeams.First(t => t.Team.Id == team.Id);
            teamToSelect.IsSelected = !teamToSelect.IsSelected;
        }

        private bool IsTeamFullySelected(TeamVM team)
        {
            TeamVM availableTeam = _availableTeams.First(t => t.Team.Id == team.Id).Team;
            TeamVM assignedTeam = _assignedTeams.FirstOrDefault(t => t.Id == team.Id);

            if (assignedTeam == null)
            {
                return false;
            }

            if (!availableTeam.Users.Any())
            {
                return false;
            }

            var result = availableTeam.Users.All(au => assignedTeam.Users.Any(asu => asu.Id == au.Id));

            return result;
        }

        private bool IsUserFromTeamSelected(TeamVM team, UserVM user)
        {
            TeamVM? assignedTeam = _assignedTeams.FirstOrDefault(ast => ast.Id == team.Id);

            if (assignedTeam == null)
            {
                return false;
            }

            return assignedTeam.Users.Any(u => u.Id == user.Id);
        }

        private void ToggleUserSelection(UserVM user, bool isSelected)
        {
            if (isSelected)
            {
                if (!_assignedModerators.Contains(user))
                {
                    _assignedModerators.Add(user);
                }
            }
            else
            {
                _assignedModerators.Remove(user);
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
                    await LoadTeamsAsync();
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
                    await LoadUsersAsync();
                    StateHasChanged();
                });
            }, null, SearchDelayMs, Timeout.Infinite);
        }

        private async Task LoadTeamsAsync()
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
                    _availableTeams = new List<SelectTeamVM>();
                    _teamsTotalCount = 0;
                    _teamsTotalPages = 0;
                    return;
                }

                _availableTeams = response.Result.Data.Select(t => new SelectTeamVM
                {
                    Team = new TeamVM
                    {
                        Id = t.Id,
                        Name = t.Name,
                        Users = t.Users!.Select(u => new UserVM
                        {
                            Id = u.Id,
                            Name = u.Name,
                            Email = u.Email,
                        }).ToList()
                    },
                    IsSelected = false
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

        private async Task LoadUsersAsync()
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
                await LoadTeamsAsync();
            }
        }

        private async Task NextTeamsPage()
        {
            if (_teamsPage < _teamsTotalPages)
            {
                _teamsPage++;
                await LoadTeamsAsync();
            }
        }

        private async Task PreviousUsersPage()
        {
            if (_usersPage > 1)
            {
                _usersPage--;
                await LoadUsersAsync();
            }
        }

        private async Task NextUsersPage()
        {
            if (_usersPage < _usersTotalPages)
            {
                _usersPage++;
                await LoadUsersAsync();
            }
        }

        private async Task SubmitAssignment()
        {
            try
            {
                if (!_assignedTeams.Any() && !_assignedModerators.Any())
                {
                    _onSubmitMessage = "Please select at least one team or user to assign.";
                    _onSubmitSuccess = false;
                    return;
                }

                var updatedStep = new StepVM
                {
                    Id = StepToAssign.Id,
                    Name = StepToAssign.Name,
                    Teams = new List<TeamVM>(),
                    Users = new List<UserVM>(),
                };

                foreach(var team in _assignedTeams)
                {
                    updatedStep.Teams.Add(team);
                }

                foreach(var user in _assignedModerators)
                {
                    updatedStep.Users.Add(user);
                }

                await StepToAssignChanged.InvokeAsync(updatedStep);

                _onSubmitMessage = "Assignment completed successfully!";
                _onSubmitSuccess = true;

                await CancelForm();

                await OnStepAssigned.InvokeAsync();
            }
            catch (Exception ex)
            {
                _onSubmitMessage = $"Error during assignment: {ex.Message}";
                _onSubmitSuccess = false;
            }
        }

        private void ClearForm()
        {
            _assignedModerators.Clear();
            _assignedTeams.Clear();
            _onSubmitMessage = string.Empty;
            _onSubmitSuccess = false;
            _isTeamsTabSelected = true;
            _teamsSearchTerm = string.Empty;
            _usersSearchTerm = string.Empty;
            _teamsPage = 1;
            _usersPage = 1;
        }

        private async Task CancelForm()
        {
            _assignedModerators.Clear();
            _assignedTeams.Clear();
            _onSubmitMessage = string.Empty;
            _onSubmitSuccess = false;
            _isTeamsTabSelected = true;
            _teamsSearchTerm = string.Empty;
            _usersSearchTerm = string.Empty;
            _teamsPage = 1;
            _usersPage = 1;

            await ShowAssignToStepModalChanged.InvokeAsync();
        }

        private int GetTotalAssignedCount()
        {
            int teamUsersCount = _assignedTeams.Sum(t => t.Users.Count);
            int individualUsersCount = _assignedModerators.Count;
            return teamUsersCount + individualUsersCount;
        }

        private bool IsTeamCompletelyAssigned(TeamVM assignedTeam)
        {
            var originalTeam = _availableTeams.FirstOrDefault(at => at.Team.Id == assignedTeam.Id)?.Team;
            if (originalTeam == null) return false;

            return assignedTeam.Users.Count == originalTeam.Users.Count;
        }

        private int GetOriginalTeamUserCount(TeamVM assignedTeam)
        {
            var originalTeam = _availableTeams.FirstOrDefault(at => at.Team.Id == assignedTeam.Id)?.Team;
            return originalTeam?.Users.Count ?? 0;
        }

        private void RemoveTeamFromAssignment(Guid teamId)
        {
            _assignedTeams.RemoveAll(t => t.Id == teamId);
            _onSubmitMessage = string.Empty;
            StateHasChanged();
        }

        private void RemoveUserFromAssignment(Guid userId)
        {
            _assignedModerators.RemoveAll(u => u.Id == userId);
            _onSubmitMessage = string.Empty;
            StateHasChanged();
        }

        private void RemoveUserFromTeamAssignment(Guid teamId, Guid userId)
        {
            var team = _assignedTeams.FirstOrDefault(t => t.Id == teamId);
            if (team != null)
            {
                team.Users.RemoveAll(u => u.Id == userId);

                if (!team.Users.Any())
                {
                    _assignedTeams.Remove(team);
                }
            }

            _onSubmitMessage = string.Empty;
            StateHasChanged();
        }
    }
}
