using FlowManager.Client.DTOs;
using FlowManager.Client.Services;
using FlowManager.Client.ViewModels;
using FlowManager.Client.ViewModels.Team;
using FlowManager.Shared.DTOs.Requests;
using FlowManager.Shared.DTOs.Requests.Team;
using FlowManager.Shared.DTOs.Responses;
using FlowManager.Shared.DTOs.Responses.Team;
using Microsoft.AspNetCore.Components;
using System.Collections;

namespace FlowManager.Client.Components.Admin.Members.ViewTeams
{
    public partial class ViewTeamsModal : ComponentBase
    {
        [Inject] private TeamService _teamService { get; set; } = default!;
        private List<TeamVM> _teams = new();
        private BitArray _dropdownTeamMembersState = new(0);
        private string _searchTerm = string.Empty;

        private bool _showAddTeamModal = false;

        private bool _showEditTeamModal = false;
        private TeamVM _teamToEdit = null;

        protected override async Task OnInitializedAsync()
        {
            await LoadTeams();
        }

        private async Task LoadTeams()
        {
            QueriedTeamRequestDto payload = null;
            if (!string.IsNullOrEmpty(_searchTerm))
            {
                payload = new QueriedTeamRequestDto
                {
                    GlobalSearchTerm = _searchTerm
                };
            }

            ApiResponse<PagedResponseDto<TeamResponseDto>> response = await _teamService.GetAllTeamsQueriedAsync(payload);

            if (response.Success)
            {
                _teams = response.Result.Data.Select(t => new TeamVM
                {
                    Id = t.Id,
                    Name = t.Name!,
                    Users = t.Users?.Select(u => new UserVM
                    {
                        Id = u.Id,
                        Name = u.Name!,
                        Email = u.Email!
                    }).ToList(),
                }).ToList();

                _dropdownTeamMembersState = new BitArray(_teams.Count, false);
            }
            else
            {
                _teams.Clear();
                _dropdownTeamMembersState = new BitArray(0);
            }
        }

        private void ChangeVisibilityTeamMembersDropdown(int i)
        {
            if (i >= 0 && i < _dropdownTeamMembersState.Length && i < _teams.Count)
            {
                _dropdownTeamMembersState[i] = !_dropdownTeamMembersState[i];
            }
        }

        private void OpenEditTeamModal(TeamVM teamToEdit)
        {
            _teamToEdit = teamToEdit;
            _showAddTeamModal = false;
            _showEditTeamModal = true;
        }


        private void OpenAddTeamModal()
        {
            _showAddTeamModal = true;
            _showEditTeamModal = false;
        }
    }
}
