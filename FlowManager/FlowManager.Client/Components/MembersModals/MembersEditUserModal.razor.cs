using FlowManager.Client.DTOs;
using FlowManager.Client.Services;
using FlowManager.Client.ViewModels;
using FlowManager.Shared.DTOs.Requests.User;
using FlowManager.Shared.DTOs.Responses.Role;
using FlowManager.Shared.DTOs.Responses.User;
using Microsoft.AspNetCore.Components;

namespace FlowManager.Client.Components.MembersModals
{
    public partial class MembersEditUserModal: ComponentBase
    {
        [Inject] private UserService _userService { get; set; } = default!;
        [Inject] private RoleService _roleService { get; set; } = default!;

        [Parameter] public bool ShowEditForm { get; set; }
        [Parameter] public UserVM UserToEdit { get; set; }

        private List<RoleVM> _availableRoles;

        private bool _isAdmin
        {
            get => UserToEdit.Roles.Any(r => r.RoleName.ToUpper() == "ADMIN");
            set
            {
                var adminRole = UserToEdit.Roles.FirstOrDefault(r => r.RoleName.ToUpper() == "ADMIN");
                if (adminRole == null && value)
                {
                    UserToEdit.Roles.Add(new RoleVM
                    {
                        Id = _availableRoles.FirstOrDefault(r => r.RoleName.ToUpper() == "ADMIN")?.Id ?? Guid.Empty,
                        RoleName = "Admin"
                    });
                }
                else if (adminRole != null && !value)
                {
                    UserToEdit.Roles.Remove(adminRole);
                }
            }
        }

        private bool _isModerator
        {
            get => UserToEdit.Roles.Any(r => r.RoleName.ToUpper() == "MODERATOR");
            set
            {
                var moderatorRole = UserToEdit.Roles.FirstOrDefault(r => r.RoleName.ToUpper() == "MODERATOR");
                if (value && moderatorRole == null)
                {
                    UserToEdit.Roles.Add(new RoleVM
                    {
                        Id = _availableRoles.FirstOrDefault(r => r.RoleName.ToUpper() == "MODERATOR")?.Id ?? Guid.Empty,
                        RoleName = "MODERATOR"
                    });
                }
                else if (moderatorRole != null && !value)
                {
                    UserToEdit.Roles.Remove(moderatorRole);
                }
            }
        }

        protected override async Task OnInitializedAsync()
        {
            ApiResponse<List<RoleResponseDto>> response = await _roleService.GetAllRolesAsync();

            _availableRoles = response.Result.Select(r => new RoleVM
            {
                Id = r.Id,
                RoleName = r.Name!
            }).ToList();
        }

        private async Task EditUser()
        {
            ApiResponse<UserResponseDto> response = await _userService.PatchUserAsync(UserToEdit.Id, new PatchUserRequestDto
            {
                Name = UserToEdit.Name,
                Email = UserToEdit.Email,
                UserName = UserToEdit.Email,
                Roles = UserToEdit.Roles.Select(r => r.Id).ToList()
            });
        }
    }
}
