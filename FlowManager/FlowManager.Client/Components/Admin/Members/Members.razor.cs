using FlowManager.Client.DTOs;
using FlowManager.Client.Services;
using FlowManager.Client.ViewModels;
using FlowManager.Shared.DTOs.Responses;
using FlowManager.Shared.DTOs.Responses.User;
using Microsoft.AspNetCore.Components;

namespace FlowManager.Client.Components.Admin.Members
{
    public partial class Members : ComponentBase
    {
        [Inject] private UserService UserService { get; set; } = default!;

        private bool _showAddForm = false;
        private bool _showEditForm = false;

        private List<UserVM> _users = new();

        private List<RoleVM> _availableRoles = new();

        private UserVM _selectedUserToEdit = new();

        protected override async Task OnInitializedAsync()
        {
            await LoadUsers();
        }

        private async Task LoadUsers()
        {
            ApiResponse<PagedResponseDto<UserResponseDto>> response = await UserService.GetAllUsersQueriedAsync();

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

        private void OpenEditUser(UserVM user)
        {
            _selectedUserToEdit = user;

            _showEditForm = true;
        }

        private void DeleteUser(UserVM user)
        {

        }

        private void RestoreUser(UserVM user)
        {

        }

        private void EditUser(UserVM user)
        {
            _showEditForm = true;
        }
    }
}
