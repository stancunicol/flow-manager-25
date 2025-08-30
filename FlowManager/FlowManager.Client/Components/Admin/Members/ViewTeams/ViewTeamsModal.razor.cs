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

        private int _pageSize = 8;
        private int _currentPage = 1;
        private int _totalPages = 0;
        private int _maxVisiblePages = 4;
        private int _totalCount = 0;

        protected override async Task OnInitializedAsync()
        {
            await LoadTeamsAsync();
        }

        private async Task LoadTeamsAsync()
        {
            QueriedTeamRequestDto payload = new QueriedTeamRequestDto()
                ;
            if (!string.IsNullOrEmpty(_searchTerm))
            {
                payload.GlobalSearchTerm = _searchTerm;
            }

            if(_pageSize != 0 && _currentPage != 0)
            {
                payload.QueryParams = new QueryParamsDto
                {
                    Page = _currentPage,
                    PageSize = _pageSize
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
                        Email = u.Email!,
                        Step = new StepVM
                        {
                            Id = u.Step!.Id,
                            Name = u.Step!.Name
                        }
                    }).ToList(),
                    IsActive = t.DeletedAt == null
                }).ToList();

                _dropdownTeamMembersState = new BitArray(_teams.Count, false);

                _totalPages = response.Result.TotalPages;
                _totalCount = response.Result.TotalCount;
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

        private async Task GoToFirstPage()
        {
            _currentPage = 1;
            await LoadTeamsAsync();
        }

        private async Task GoToPreviousPage()
        {
            _currentPage--;
            await LoadTeamsAsync();
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
            await LoadTeamsAsync();
        }

        private async Task GoToNextPage()
        {
            _currentPage++;
            await LoadTeamsAsync();
        }

        private async Task GoToLastPage()
        {
            _currentPage = _totalPages;
            await LoadTeamsAsync();
        }

        private async Task DeleteTeam(Guid teamId)
        {
            await _teamService.DeleteTeamAsync(teamId);
            await LoadTeamsAsync();
        }

        private async Task RestoreTeam(Guid teamId)
        {
            await _teamService.RestoreTeamAsync(teamId);
            await LoadTeamsAsync();
        }
    }
}
