using FlowManager.Client.DTOs;
using FlowManager.Client.Services;
using FlowManager.Client.ViewModels;
using FlowManager.Shared.DTOs.Requests.User;
using FlowManager.Shared.DTOs.Responses.Role;
using FlowManager.Shared.DTOs.Responses.User;
using Microsoft.AspNetCore.Components;

namespace FlowManager.Client.Components.MembersModals
{
    public partial class MembersAddUserModal : ComponentBase
    {
        [Inject] private UserService _userService { get; set; } = default!;
        [Inject] private RoleService _roleService { get; set; } = default!;

        [Parameter] public bool ShowAddForm { get; set; }

        // add/edit a user
        private string _name = "";
        private string _email = "";
        private List<Guid> selectedRoles = new();

        private List<RoleVM> _availableRoles;

        private bool _isNewUserAdmin = false;
        private bool _isNewUserModerator = false;

        protected override async Task OnInitializedAsync()
        {
            ApiResponse<List<RoleResponseDto>> response = await _roleService.GetAllRolesAsync();

            _availableRoles = response.Result.Select(r => new RoleVM
            {
                Id = r.Id,
                RoleName = r.Name!
            }).ToList();
        }

        private async Task RegisterUser()
        {
            ApiResponse<UserResponseDto> response = await _userService.PostUserAsync(new PostUserRequestDto
            {
                Email = _email,
                Name = _name,
                Username = _email,
                Roles = selectedRoles
            });
        }
    }
}
