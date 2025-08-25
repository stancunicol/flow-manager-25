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
using Microsoft.AspNetCore.Components.Web;
using System.Linq;

namespace FlowManager.Client.Components.Admin.Members.ViewTeams.AddEditTeamsModals
{
    public partial class EditTeamModal : ComponentBase
    {
        [Parameter] public bool ShowEditTeamModal { get; set; }
        [Parameter] public EventCallback<bool> ShowEditTeamModalChanged { get; set; }
        [Parameter] public TeamVM TeamToEdit { get; set; }
        [Parameter] public EventCallback TeamWasEdited { get; set; }
 
        [Inject] private UserService _userService { get; set; } = default!;
        [Inject] private TeamService _teamService { get; set; } = default!;

        private string _searchTerm { get; set; } = string.Empty;

        private string _teamName { get; set; } = string.Empty;

        private bool _isSubmitting { get; set; } = false;
        private string _submitMessage { get; set; } = string.Empty;
        private bool _submitStatus { get; set; } = false;

        private List<UserVM> _assignedUsers = new List<UserVM>();
        private List<UserVM> _filteredUsers = new List<UserVM>();

        private bool _showAssignedUsersTab { get; set; } = true;

        protected override async Task OnInitializedAsync()
        {
            await LoadUsers(reset: true);
            _teamName = TeamToEdit.Name!;
        }

        private async Task LoadUsers(bool reset = false)
        {
            QueriedTeamRequestDto payload = null;
            if (!string.IsNullOrEmpty(_searchTerm))
            { 
                payload = new QueriedTeamRequestDto
                { 
                    GlobalSearchTerm = _searchTerm
                };
            }

            ApiResponse<SplitUsersByTeamIdResponseDto> response = await _teamService.GetSplitUsersByTeamIdAsync(TeamToEdit.Id, payload);

            if (!response.Success)
            {
                _assignedUsers.Clear();
                _filteredUsers.Clear();
                return;
            }

            if(reset)
            {
                _assignedUsers = response.Result.AssignedToTeamUsers.Select(u => new UserVM
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                }).ToList();
            }

            _filteredUsers = response.Result.AssignedToTeamUsers.Concat(response.Result.UnassignedToTeamUsers).Select(u => new UserVM
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
            }).ToList();
        }

        private async Task OnCancel()
        {
            _searchTerm = string.Empty;
            _teamName = string.Empty;

            await ShowEditTeamModalChanged.InvokeAsync(false);
        }

        private async Task OnSubmit()
        {
            await EditTeam();
            await LoadUsers();
        }

        private async Task EditTeam()
        {
            _isSubmitting = true;

            PatchTeamRequestDto payload = new PatchTeamRequestDto();

            if(!string.IsNullOrEmpty(_teamName))
            {
                payload.Name = _teamName;
            }

            if(_assignedUsers != null && _assignedUsers.Count > 0)
            {
                payload.UserIds = _assignedUsers.Select(u => u.Id).ToList();
            }

            _isSubmitting = false;

            ApiResponse<TeamResponseDto> result = await _teamService.PatchTeamAsync(TeamToEdit.Id, payload);

            _submitStatus = result.Success;
            _submitMessage = result.Message;

            if(_submitStatus)
            {
                await TeamWasEdited.InvokeAsync();
            }
        }

        private bool IsUserAssigned(UserVM user)
        {
            return _assignedUsers.Any(u => u.Id == user.Id);
        }

        private void UserAssignStateChanged(ChangeEventArgs e, UserVM user)
        {
            var existingUser = _assignedUsers.FirstOrDefault(u => u.Id == user.Id);

            if (existingUser != null)
            {
                _assignedUsers.Remove(existingUser);
            }
            else
            {
                _assignedUsers.Add(user);
            }
        }

        private async Task HandleSearchKeyPress(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
            {
                await LoadUsers();
            }
        }
    }
}
