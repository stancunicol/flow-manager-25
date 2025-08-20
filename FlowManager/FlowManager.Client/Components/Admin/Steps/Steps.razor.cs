using FlowManager.Client.DTOs;
using FlowManager.Client.ViewModels;
using FlowManager.Shared.DTOs.Responses.User;
using FlowManager.Shared.DTOs.Responses;
using Microsoft.AspNetCore.Components;
using FlowManager.Client.Services;

namespace FlowManager.Client.Components.Admin.Steps
{
    public partial class Steps: ComponentBase
    {
        [Inject] private UserService _userService { get; set; } = default!;

        private List<UserVM> _users = new();

        protected override async Task OnInitializedAsync()
        {
            await LoadUsers();
        }

        private async Task LoadUsers()
        {
            ApiResponse<PagedResponseDto<UserResponseDto>> response = await _userService.GetAllUsersQueriedAsync();

            if (!response.Success)
            {
                _users = new();
                return;
            }

            _users = response.Result.Data.Where(u => u.Roles != null && u.Roles.Any(r => r.Name != null && r.Name.ToUpper() == "MODERATOR")).Select(u => new UserVM
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
    }
}