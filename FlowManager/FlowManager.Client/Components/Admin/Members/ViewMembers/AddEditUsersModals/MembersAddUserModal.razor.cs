using FlowManager.Client.DTOs;
using FlowManager.Client.Services;
using FlowManager.Client.ViewModels;
using FlowManager.Shared.DTOs.Requests.User;
using FlowManager.Shared.DTOs.Responses;
using FlowManager.Shared.DTOs.Responses.Role;
using FlowManager.Shared.DTOs.Responses.Step;
using FlowManager.Shared.DTOs.Responses.User;
using Microsoft.AspNetCore.Components;
using System.Numerics;
using System.Text.RegularExpressions;

namespace FlowManager.Client.Components.Admin.Members.ViewMembers.AddEditUsersModals
{
    public partial class MembersAddUserModal : ComponentBase
    {
        [Inject] private UserService _userService { get; set; } = default!;
        [Inject] private RoleService _roleService { get; set; } = default!;
        [Inject] private StepService _stepService { get; set; } = default!;

        [Parameter] public bool ShowAddForm { get; set; }
        [Parameter] public EventCallback<bool> ShowAddFormChanged { get; set; }

        [Parameter] public EventCallback OnUserAdded { get; set; }

        private string _onSubmitMessage = string.Empty;
        private bool _onSubmitSuccess;

        [Inject] private ILogger<MembersAddUserModal> _logger { get; set; } = default!;

        private string _name = "";
        private string _email = "";
        private string _phoneNumber = "";
        private List<Guid> selectedRoles = new();
        private Guid _selectedStepId = Guid.Empty;
        private bool _isDropdownOpen = false;
        private string? _selectedStepName;

        private List<RoleVM> _availableRoles = new();
        private List<StepVM> _availableSteps = new();

        private bool _isNewUserAdmin = false;
        private bool _isNewUserModerator = false;

        protected override async Task OnInitializedAsync()
        {
            await LoadRolesAsync();
            await LoadStepsAsync();
        }

        private async Task RegisterUser()
        {
            if (_isNewUserAdmin)
            {
                selectedRoles.Add(_availableRoles.First(r => r.RoleName.ToUpper() == "ADMIN").Id);
            }

            if (_isNewUserModerator)
            {
                selectedRoles.Add(_availableRoles.First(r => r.RoleName.ToUpper() == "MODERATOR").Id);
            }

            ApiResponse<UserResponseDto> response = await _userService.PostUserAsync(new PostUserRequestDto
            {
                Email = _email,
                Name = _name,
                Username = _email,
                PhoneNumber = _phoneNumber,
                Roles = selectedRoles,
                StepId = _selectedStepId,
            });

            _onSubmitMessage = response.Message;
            _onSubmitSuccess = response.Success;
            StateHasChanged();

            if (!response.Success)
            {
                _logger.LogError("Failed to register user: {Message}", response.Message);
                return;
            }

            await Task.Delay(3000);

            ClearForm();
            _onSubmitMessage = string.Empty;
            StateHasChanged();

            await OnUserAdded.InvokeAsync();
        }

        private void ToggleDropdown()
        {
            _isDropdownOpen = !_isDropdownOpen;
        }

        private void SelectStep(StepVM step)
        {
            _selectedStepId = step.Id ?? Guid.Empty;
            _selectedStepName = step.Name;
            _isDropdownOpen = false;
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
            ApiResponse<PagedResponseDto<StepResponseDto>> response = await _stepService.GetAllStepsIncludeUsersAndTeamsQueriedAsync();

            if(!response.Success)
            {
                _availableSteps = new();
            }

            _availableSteps = response.Result.Data.Select(s => new StepVM
            {
                Id = s.StepId,
                Name = s.StepName
            }).ToList();
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
            _phoneNumber = string.Empty;
            selectedRoles.Clear();
            _isNewUserAdmin = false;
            _isNewUserModerator = false;
        }

        private bool IsSubmitValid()
        {
            return !string.IsNullOrEmpty(_name) && !string.IsNullOrEmpty(_email) && !string.IsNullOrEmpty(_selectedStepName) && IsPhoneNumberValid(); 
        }

        private bool IsPhoneNumberValid()
        {
            if (string.IsNullOrEmpty(_phoneNumber))
            {
                return false; 
            }

            Regex phoneNumberRegex = new Regex(@"^\+[1-9][0-9]{7,14}");

            return phoneNumberRegex.IsMatch(_phoneNumber);
        }
    }
}