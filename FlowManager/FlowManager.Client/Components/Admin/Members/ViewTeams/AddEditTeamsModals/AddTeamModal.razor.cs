using FlowManager.Client.DTOs;
using FlowManager.Client.Services;
using FlowManager.Client.ViewModels;
using FlowManager.Shared.DTOs.Requests;
using FlowManager.Shared.DTOs.Requests.Team;
using FlowManager.Shared.DTOs.Requests.User;
using FlowManager.Shared.DTOs.Responses;
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

        private string _searchTerm { get; set; } = string.Empty;

        private string _teamName { get; set; } = string.Empty; 

        private bool _isSubmitting { get; set; } = false;
        private string _submitMessage { get; set; } = string.Empty;
        private bool _submitStatus { get; set; } = false;

        private List<UserVM> _users = new List<UserVM>();
        private bool[] _selectionStateUser = new bool[0];

        private int _currentPage = 1;
        private int _pageSize = 10;
        private int _totalPages = 0;
        private int _totalCount = 0;

        protected override async Task OnInitializedAsync()
        {
            await LoadUsers();
        }

        private async Task LoadUsers(bool resetPageSize = false)
        {
            QueriedUserRequestDto payload = new QueriedUserRequestDto(); 

            if(!string.IsNullOrEmpty(_searchTerm))
            {
                payload.GlobalSearchTerm = _searchTerm;
            }

            if(resetPageSize)
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

            ApiResponse<PagedResponseDto<UserResponseDto>> response = await _userService.GetAllUsersQueriedAsync(payload);

            if(!response.Success)
            {
                _users = new List<UserVM>();
                _selectionStateUser = new bool[0];
                return;
            }

            _users = response.Result.Data.Select(u => new UserVM
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email
            }).ToList();

            _selectionStateUser = new bool[_users.Count];

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
            await PostTeam();
            ClearForm();
            await LoadUsers();
        }

        private async Task PostTeam()
        {
            _isSubmitting = true;
            PostTeamRequestDto payload = new PostTeamRequestDto
            {
                Name = _teamName,
                UserIds = _selectionStateUser.Where(state => state == true).Select((selectedUser, index) => _users[index].Id).ToList()
            };

            _isSubmitting = false;

            ApiResponse<TeamResponseDto> result = await _teamService.PostTeamAsync(payload);

            _submitStatus = result.Success;
            _submitMessage = result.Message;

            if(_submitStatus)
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
            await LoadUsers();
        }
    }
}
