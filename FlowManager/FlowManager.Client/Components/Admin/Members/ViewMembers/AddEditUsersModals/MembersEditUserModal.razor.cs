using FlowManager.Client.DTOs;
using FlowManager.Client.Services;
using FlowManager.Client.ViewModels;
using FlowManager.Shared.DTOs.Requests.Step;
using FlowManager.Shared.DTOs.Requests.User;
using FlowManager.Shared.DTOs.Responses;
using FlowManager.Shared.DTOs.Responses.Role;
using FlowManager.Shared.DTOs.Responses.Step;
using FlowManager.Shared.DTOs.Responses.User;
using Microsoft.AspNetCore.Components;

namespace FlowManager.Client.Components.Admin.Members.ViewMembers.AddEditUsersModals
{
    public partial class MembersEditUserModal : ComponentBase
    {
        [Inject] private UserService _userService { get; set; } = default!;
        [Inject] private RoleService _roleService { get; set; } = default!;
        [Inject] private StepService _stepService { get; set; } = default!; 

        [Parameter] public bool ShowEditForm { get; set; }
        [Parameter] public EventCallback<bool> ShowEditFormChanged { get; set; }
        [Parameter] public UserVM UserToEdit { get; set; }

        [Parameter] public EventCallback OnUserEdit { get; set; }

        private string _onSubmitMessage = string.Empty;
        private bool _onSubmitSuccess;

        private List<RoleVM> _availableRoles = new();
        private List<StepVM> _availableSteps = new();

        private bool _isDropdownOpen = false;

        private bool _isAdmin
        {
            get => UserToEdit.Roles!.Any(r => r.RoleName.ToUpper() == "ADMIN");
            set
            {
                var adminRole = UserToEdit.Roles?.FirstOrDefault(r => r.RoleName.ToUpper() == "ADMIN");
                if (adminRole == null && value)
                {
                    UserToEdit.Roles?.Add(new RoleVM
                    {
                        Id = _availableRoles.FirstOrDefault(r => r.RoleName.ToUpper() == "ADMIN")?.Id ?? Guid.Empty,
                        RoleName = "Admin"
                    });
                }
                else if (adminRole != null && !value)
                {
                    UserToEdit.Roles?.Remove(adminRole);
                }
            }
        }

        private bool _isModerator
        {
            get => UserToEdit.Roles!.Any(r => r.RoleName.ToUpper() == "MODERATOR");
            set
            {
                var moderatorRole = UserToEdit.Roles?.FirstOrDefault(r => r.RoleName.ToUpper() == "MODERATOR");
                if (value && moderatorRole == null)
                {
                    UserToEdit.Roles?.Add(new RoleVM
                    {
                        Id = _availableRoles.FirstOrDefault(r => r.RoleName.ToUpper() == "MODERATOR")?.Id ?? Guid.Empty,
                        RoleName = "MODERATOR"
                    });
                }
                else if (moderatorRole != null && !value)
                {
                    UserToEdit.Roles?.Remove(moderatorRole);
                }
            }
        }

        protected override async Task OnInitializedAsync()
        {
            await LoadRolesAsync();
            await LoadStepsAsync();
            Console.WriteLine($"{UserToEdit.Step?.Name}");
        }

        private async Task LoadRolesAsync()
        {
            ApiResponse<List<RoleResponseDto>> response = await _roleService.GetAllRolesAsync();

            _availableRoles = response.Result.Select(r => new RoleVM
            {
                Id = r.Id,
                RoleName = r.Name!
            }).ToList();
        }

        private async Task LoadStepsAsync()
        {
            ApiResponse<PagedResponseDto<StepResponseDto>> response = await _stepService.GetStepsQueriedAsync();

            _availableSteps = response.Result.Data.Select(s => new StepVM
            {
                Id = s.Id,
                Name = s.Name,
            }).ToList();
        }

        private async Task EditUser()
        {
            ApiResponse<UserResponseDto> response = await _userService.PatchUserAsync(UserToEdit.Id, new PatchUserRequestDto
            {
                Name = UserToEdit.Name,
                Email = UserToEdit.Email,
                UserName = UserToEdit.Email,
                PhoneNumber = UserToEdit.PhoneNumber,
                StepId = UserToEdit.Step?.Id,
                Roles = UserToEdit.Roles?.Select(r => r.Id).ToList()
            });

            _onSubmitSuccess = response.Success;
            _onSubmitMessage = response.Message;
            StateHasChanged();

            if (!response.Success)
            {
                return;
            }

            await Task.Delay(3000);

            _onSubmitMessage = string.Empty;
            StateHasChanged();

            await OnUserEdit.InvokeAsync();
        }

        private async Task CancelForm()
        {
            await ShowEditFormChanged.InvokeAsync(false);
        }

        private void ToggleDropdown()
        {
            _isDropdownOpen = !_isDropdownOpen;
        }

        private void SelectStep(StepVM step)
        {
            UserToEdit.Step!.Id = step.Id;
            UserToEdit.Step.Name = step.Name;
            _isDropdownOpen = false;
        }
    }
}
