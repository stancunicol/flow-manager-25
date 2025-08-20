using FlowManager.Client.DTOs;
using FlowManager.Client.Services;
using FlowManager.Client.ViewModels;
using FlowManager.Shared.DTOs.Requests.User;
using FlowManager.Shared.DTOs.Responses.Role;
using FlowManager.Shared.DTOs.Responses.User;
using Microsoft.AspNetCore.Components;

namespace FlowManager.Client.Components.Admin.Members.MembersModals
{
    public partial class MembersAddUserModal : ComponentBase
    {
        [Inject] private UserService _userService { get; set; } = default!;
        [Inject] private RoleService _roleService { get; set; } = default!;
        [Parameter] public bool ShowAddForm { get; set; }
        [Parameter] public EventCallback<bool> ShowAddFormChanged { get; set; }
        [Parameter] public EventCallback OnUserAdded { get; set; }
        private string _onSubmitMessage = string.Empty;
        private bool _onSubmitSuccess;
        [Inject] private ILogger<MembersAddUserModal> _logger { get; set; } = default!;

        // add/edit a user
        private string _name = "";
        private string _email = "";
        private string _selectedRole = "employee";
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

        private void OnRoleChange(string role)
        {
            _selectedRole = role;
            selectedRoles.Clear();
            _isNewUserAdmin = false;
            _isNewUserModerator = false;

            if (role == "admin")
                _isNewUserAdmin = true;
            else if (role == "moderator")
                _isNewUserModerator = true;
        }

        private async Task RegisterUser()
        {
            selectedRoles.Clear();

            var employeeRole = _availableRoles.FirstOrDefault(r => r.RoleName.ToUpper() == "EMPLOYEE");
            if (employeeRole != null)
            {
                selectedRoles.Add(employeeRole.Id);
            }

            if (_isNewUserAdmin)
            {
                selectedRoles.Add(_availableRoles.First(r => r.RoleName.ToUpper() == "ADMIN").Id);
            }
            else if (_isNewUserModerator)
            {
                selectedRoles.Add(_availableRoles.First(r => r.RoleName.ToUpper() == "MODERATOR").Id);
            }


            ApiResponse<UserResponseDto> response = await _userService.PostUserAsync(new PostUserRequestDto
            {
                Email = _email,
                Name = _name,
                Username = _email,
                Roles = selectedRoles
            });

            _onSubmitMessage = response.Message;
            _onSubmitSuccess = response.Success;

            if (!response.Success)
            {
                _logger.LogError("Failed to register user: {Message}", response.Message);
                return;
            }

            await OnUserAdded.InvokeAsync();
        }

        private async Task CancelForm()
        {
            await ShowAddFormChanged.InvokeAsync(false);
            ClearForm();
        }

        private void ClearForm()
        {
            _name = string.Empty;
            _email = string.Empty;
            _selectedRole = "employee";
            selectedRoles.Clear();
            _isNewUserAdmin = false;
            _isNewUserModerator = false;
        }
    }
}