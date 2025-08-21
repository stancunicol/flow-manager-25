using FlowManager.Client.DTOs;
using FlowManager.Client.Services;
using FlowManager.Client.ViewModels;
using FlowManager.Shared.DTOs.Requests.User;
using FlowManager.Shared.DTOs.Responses;
using FlowManager.Shared.DTOs.Responses.User;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace FlowManager.Client.Components.Admin.Members
{
    public partial class Members : ComponentBase
    {
        [Inject] private UserService _userService { get; set; } = default!;
        [Inject] private ILogger<Members> _logger { get; set; } = default!;

        private bool _showAddForm = false;
        private bool _showEditForm = false;

        private List<UserVM> _users = new();

        private UserVM _selectedUserToEdit = new();

        private string _searchTerm = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            await LoadUsers();
        }

        private async Task LoadUsers()
        {
            QueriedUserRequestDto payload = new QueriedUserRequestDto();

            if(!string.IsNullOrEmpty(_searchTerm))
            {
                payload.Email = _searchTerm;
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
                IsActive = u.DeletedAt == null,
                Roles = u.Roles!.Select(r => new RoleVM
                {
                    Id = r.Id,
                    RoleName = r.Name!
                }).ToList(),
            }).ToList();
        }

        private async Task DeleteUser(UserVM user)
        {
            ApiResponse<UserResponseDto> response = await _userService.DeleteUserAsync(user.Id);

            if(!response.Success)
            {
                _logger.LogError("Failed to delete user {UserId}: {Message}", user.Id, response.Message);
            }

            await LoadUsers();
        }

        private async Task RestoreUser(UserVM user)
        {
            ApiResponse<UserResponseDto> response = await _userService.RestoreUserAsync(user.Id);

            if(!response.Success)
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
                _searchTerm = _searchTerm.Trim();
                await LoadUsers();
            }
        }

        private async Task SearchFlows()
        {
            _searchTerm = _searchTerm.Trim();
            await LoadUsers();
        }
    }
}
