using FlowManager.Client.DTOs;
using FlowManager.Client.ViewModels;
using FlowManager.Shared.DTOs.Responses.User;
using FlowManager.Shared.DTOs.Responses;
using Microsoft.AspNetCore.Components;
using FlowManager.Client.Services;
using FlowManager.Application.Services;
using FlowManager.Domain.Entities;
using FlowManager.Shared.DTOs.Responses.Step;
using StepService = FlowManager.Client.Services.StepService;
using UserService = FlowManager.Client.Services.UserService;
using TeamService = FlowManager.Client.Services.TeamService;
using StepHistoryService = FlowManager.Client.Services.StepHistoryService;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Linq.Expressions;
using FlowManager.Shared.DTOs.Requests.Step;
using FlowManager.Shared.DTOs.Requests.Team;
using Microsoft.AspNetCore.Components.Web;
using FlowManager.Shared.DTOs.Responses.Team;
using FlowManager.Shared.DTOs.Requests.StepHistory;

namespace FlowManager.Client.Components.Admin.Steps
{
    public partial class Steps : ComponentBase
    {
        private List<StepResponseDto> departments = new();
        private bool isModalOpen = false;
        private bool isCreateModalOpen = false;
        private bool isDeleteModalOpen = false;
        private bool isMoveUsersModalOpen = false;
        private bool isChangeUsersModalOpen = false;
        private bool isMoveModalOpen = false;
        private Guid selectedReassignDepartmentId { get; set; }
        private StepResponseDto? selectedDepartment;
        private StepResponseDto? departmentToDelete;
        private StepResponseDto? departmentToMove;
        private string newDepName = string.Empty;
        private string error = string.Empty;
        private List<UserResponseDto> unsignedUsers = new();
        private List<UserResponseDto> selectedUsers = new();
        private bool isEditModalOpen = false;
        private string editDepName = string.Empty;
        private List<UserResponseDto> allUsersList = new();
        private List<UserResponseDto> allUsers = new();
        private List<TeamResponseDto> allTeams = new();
        private Dictionary<Guid, string> departmentColors = new();
        private Guid draggedUserId = Guid.Empty;
        private List<TeamResponseDto> selectedTeams = new();
        private bool isEditTypeModalOpen = false;
        private enum EditType { None, ChangeName, MoveUsers }
        private EditType currentEditType = EditType.None;
        private bool isSelectMoveDepartmentModalOpen = false;
        private bool isEditDropdownOpen = false;

        [Inject]
        private StepService stepService { get; set; } = default!;
        [Inject]
        private UserService userService { get; set; } = default!;
        [Inject]
        private TeamService teamService { get; set; } = default!;
        [Inject]
        private StepHistoryService stepHistoryService { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            await LoadDepartments();
        }

        private async Task LoadDepartments()
        {
            try
            {
                var response = await stepService.GetStepsQueriedAsync();
                if (response != null && response.Success && response.Result != null)
                {
                    departments = response.Result.Data
                        .Where(d => d.DeletedAt == null)
                        .ToList();

                    for (int i = 0; i < departments.Count; i++)
                    {
                        var departmentDetails = await stepService.GetStepAsync(departments[i].Id);
                        if (departmentDetails != null)
                        {
                            departments[i] = departmentDetails;
                        }

                        departments[i].Users ??= new List<UserResponseDto>();
                        departments[i].Teams ??= new List<TeamResponseDto>();

                        if (!departmentColors.ContainsKey(departments[i].Id))
                        {
                            departmentColors[departments[i].Id] = GetRandomGradient();
                        }

                        Console.WriteLine($"Department {departments[i].Name}: {departments[i].Users.Count} users, {departments[i].Teams.Count} teams");
                    }

                    var toRemove = departmentColors.Keys
                        .Where(k => !departments.Any(d => d.Id == k))
                        .ToList();
                    foreach (var key in toRemove)
                    {
                        departmentColors.Remove(key);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading departments: {ex.Message}");
            }
        }

        private async Task OpenModal(StepResponseDto department)
        {
            isModalOpen = true;
            var stepDetails = await stepService.GetStepAsync(department.Id);

            if (stepDetails != null)
            {
                Console.WriteLine($"Loaded step: {stepDetails.Name} ({stepDetails.Id})");
                selectedDepartment = stepDetails;
            }
            else
            {
                Console.WriteLine("Step details not found!");
            }
        }

        private async void OpenCreateModal()
        {
            newDepName = string.Empty;
            error = string.Empty;
            isCreateModalOpen = true;
            StateHasChanged();
        }

        private void CloseCreateModal()
        {
            isCreateModalOpen = false;
            error = string.Empty;
            newDepName = string.Empty;
            isEditDropdownOpen = false;
        }

        private void CloseModal()
        {
            isModalOpen = false;
            selectedDepartment = null;
            isEditDropdownOpen = false;
        }

        private async Task SaveDepartment()
        {
            if (string.IsNullOrWhiteSpace(newDepName))
            {
                error = "Write a name.";
                return;
            }

            if (departments.Any(d => d.Name.Equals(newDepName.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                Console.WriteLine("A department with this name already exists!");
                error = "This department name already exists. Try another.";
                return;
            }

            try
            {
                var step = new Step
                {
                    Name = newDepName.Trim(),
                    DeletedAt = null,
                    FlowSteps = new List<FlowStep>(),
                    Users = new List<User>(),
                };

                var result = await stepService.CreateStepAsync(step);


                if (result != null)
                {
                    var payload = new CreateStepHistoryRequestDto
                    {
                        NewDepartmentName = newDepName,
                        StepId = result.Id
                    };

                    await stepHistoryService.CreateStepHistoryForCreateDepartmentAsync(payload);

                    await LoadDepartments();

                    await RefreshAllData();

                    newDepName = string.Empty;
                    isCreateModalOpen = false;
                    error = string.Empty;
                    StateHasChanged();

                    Console.WriteLine("✅ Department created successfully!");
                }
                else
                {
                    Console.WriteLine("❌ Failed to create department");
                    error = "Could not create department.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 Error saving step: {ex.Message}");
                error = "Unexpected error occurred.";
            }
        }

        private void ToggleUserSelection(UserResponseDto user, bool isChecked)
        {
            if (isChecked)
            {
                if (!selectedUsers.Any(u => u.Id == user.Id))
                    selectedUsers.Add(user);
            }
            else
            {
                selectedUsers.RemoveAll(u => u.Id == user.Id);
            }
        }

        private void ToggleTeamSelection(TeamResponseDto team, bool isChecked)
        {
            if (isChecked)
            {
                if (!selectedTeams.Any(t => t.Id == team.Id))
                    selectedTeams.Add(team);
            }
            else
            {
                selectedTeams.RemoveAll(t => t.Id == team.Id);
            }
        }

        private async Task RefreshAllData()
        {
            try
            {
                departments.Clear();
                unsignedUsers.Clear();

                await LoadDepartments();

                StateHasChanged();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error refreshing data: {ex.Message}");
            }
        }

        private async Task OpenSelectDepartmentModal(Guid departmentId)
        {
            var step = await stepService.GetStepAsync(departmentId);

            if (step != null)
            {
                departmentToDelete = step;

                if ((departmentToDelete.Users == null || !departmentToDelete.Users.Any()) &&
                    (departmentToDelete.Teams == null || !departmentToDelete.Teams.Any()))
                {
                    var payload = new CreateStepHistoryRequestDto
                    {
                        OldDepartmentName = departmentToDelete.Name,
                        StepId = departmentToDelete.Id
                    };

                    var result = await stepService.DeleteStepAsync(departmentToDelete.Id);
                    if (result != false)
                    {
                        await stepHistoryService.CreateStepHistoryForDeleteDepartmentAsync(payload);
                        departments.RemoveAll(d => d.Id == departmentToDelete.Id);
                    }

                    isDeleteModalOpen = false;
                    CloseSelectDepartmentModal();
                    CloseModal();
                    CloseEditModal();
                    await RefreshAllData();
                    StateHasChanged();

                    Console.WriteLine($"✅ Department '{departmentToDelete.Name}' deleted directly (no users/teams).");
                    return;
                }

                isDeleteModalOpen = true;
                CloseModal();
                CloseEditModal();
                StateHasChanged();
            }
        }


        private void CloseSelectDepartmentModal()
        {
            isDeleteModalOpen = false;
            isEditDropdownOpen = false;
            StateHasChanged();
        }

        private async Task OpenMoveUsersModal()
        {
            if (departmentToDelete == null || selectedReassignDepartmentId == Guid.Empty)
                return;

            var target = await stepService.GetStepAsync(selectedReassignDepartmentId);

            if (target != null)
            {
                selectedDepartment = target;
                isMoveUsersModalOpen = true;
                CloseSelectDepartmentModal();
                StateHasChanged();
            }
        }

        private async Task SaveEditNameDepartment()
        {
            if (selectedDepartment == null)
                return;

            try
            {
                var payload = new CreateStepHistoryRequestDto
                {
                    StepId = selectedDepartment.Id,
                    OldDepartmentName = selectedDepartment.Name,
                    NewName = editDepName.Trim()
                };

                selectedDepartment.Name = editDepName.Trim();

                var updatePayload = new PatchStepRequestDto
                {
                    Name = selectedDepartment.Name,
                    UserIds = selectedDepartment.Users.Select(u => u.Id).ToList()
                };

                await stepService.UpdateStepAsync(selectedDepartment.Id, updatePayload);

                await stepHistoryService.CreateStepHistoryForNameChangeAsync(payload);

                await RefreshAllData();
                StateHasChanged();
                CloseEditModal();

                Console.WriteLine("✅ Department name updated successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 Error updating department name: {ex.Message}");
                error = "Unexpected error occurred.";
            }
        }

        private void CloseEditModal()
        {
            isEditModalOpen = false;
            isEditDropdownOpen = false;
            selectedDepartment = null;
            editDepName = string.Empty;
            selectedUsers.Clear();
            selectedTeams.Clear();
            allUsers.Clear();
            allTeams.Clear();
        }

        private string GetRandomGradient()
        {
            var gradients = new List<string>
        {
            "linear-gradient(135deg, #f6d365, #fda085)",
            "linear-gradient(135deg, #a1c4fd, #c2e9fb)",
            "linear-gradient(135deg, #84fab0, #8fd3f4)",
            "linear-gradient(135deg, #ffecd2, #fcb69f)",
            "linear-gradient(135deg, #e0c3fc, #8ec5fc)"
        };

            var rnd = new Random();
            return gradients[rnd.Next(gradients.Count)];
        }

        private void MoveUserToTarget(UserResponseDto user)
        {
            if (departmentToDelete == null || selectedDepartment == null)
                return;

            departmentToDelete.Users?.Remove(user);

            selectedDepartment.Users ??= new List<UserResponseDto>();
            selectedDepartment.Users.Add(user);

            StateHasChanged();
        }

        private void MoveTeamToTarget(TeamResponseDto team)
        {
            if (departmentToDelete == null || selectedDepartment == null)
                return;

            departmentToDelete.Teams?.Remove(team);

            selectedDepartment.Teams ??= new List<TeamResponseDto>();
            selectedDepartment.Teams.Add(team);

            foreach (var user in team.Users)
            {
                if (!selectedDepartment.Users.Any(u => u.Id == user.Id))
                {
                    selectedDepartment.Users.Add(user);
                }
            }

            StateHasChanged();
        }

        private async Task SaveUserMoves()
        {
            try
            {
                var targetPayload = new PatchStepRequestDto
                {
                    UserIds = selectedDepartment.Users.Select(u => u.Id).ToList()
                };

                await stepService.UpdateStepAsync(selectedDepartment.Id, targetPayload);

                if (departmentToDelete.Users == null || !departmentToDelete.Users.Any())
                {
                    var payload = new CreateStepHistoryRequestDto
                    {
                        OldDepartmentName = departmentToDelete.Name,
                        StepId = departmentToDelete.Id
                    };

                    var result = await stepService.DeleteStepAsync(departmentToDelete.Id);
                    departments.RemoveAll(d => d.Id == departmentToDelete.Id);

                    if(result != false)
                    {
                        await stepHistoryService.CreateStepHistoryForDeleteDepartmentAsync(payload);
                    }

                    StateHasChanged();
                }
                else
                {
                    var deletePayload = new PatchStepRequestDto
                    {
                        UserIds = departmentToDelete.Users.Select(u => u.Id).ToList()
                    };

                    await stepService.UpdateStepAsync(departmentToDelete.Id, deletePayload);
                }

                await CloseMoveUsersModal();
                await OpenSelectDepartmentModal(departmentToDelete.Id);
                StateHasChanged();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error at saving moves: {ex.Message}");
            }
        }

        private async Task CloseMoveUsersModal()
        {
            isMoveUsersModalOpen = false;
            selectedDepartment = null;
            selectedReassignDepartmentId = Guid.Empty;
            isEditDropdownOpen = false;

            StateHasChanged();
        }

        private void OpenChangeNameModal(StepResponseDto department)
        {
            selectedDepartment = department;
            editDepName = department.Name;
            currentEditType = EditType.ChangeName;
            isEditModalOpen = true;
            StateHasChanged();
        }

   
        private void OpenChangeUsersModal(StepResponseDto department)
        {
            selectedDepartment = department;
            isChangeUsersModalOpen = true;
            StateHasChanged();
        }

        private async Task OpenMoveModal()
        {
            CloseSelectDepartmentModal();
            if (selectedDepartment == null || selectedReassignDepartmentId == Guid.Empty)
                return;

            var target = await stepService.GetStepAsync(selectedReassignDepartmentId);

            if (target != null)
            {
                departmentToMove = target;
                isMoveModalOpen = true;
                isChangeUsersModalOpen = false;
                StateHasChanged();
            }
        }

        private async Task CloseMoveModal()
        {
            isMoveModalOpen = false;
            selectedDepartment = null;
            departmentToMove = null;
            selectedReassignDepartmentId = Guid.Empty;
            await RefreshAllData();
            StateHasChanged();
        }

        private async Task SaveUserMovesWithoutDelete()
        {
            try
            {
                departmentToMove.Users ??= new List<UserResponseDto>();
                selectedDepartment.Users ??= new List<UserResponseDto>();

                Console.WriteLine($"Before save - Source: {selectedDepartment.Users.Count} users");
                Console.WriteLine($"Before save - Target: {departmentToMove.Users.Count} users");

                var sourcePayload = new PatchStepRequestDto
                {
                    Name = selectedDepartment.Name,
                    UserIds = selectedDepartment.Users.Select(u => u.Id).ToList()
                };

                await stepService.UpdateStepAsync(selectedDepartment.Id, sourcePayload);

                var targetPayload = new PatchStepRequestDto
                {
                    Name = departmentToMove.Name,
                    UserIds = departmentToMove.Users.Select(u => u.Id).ToList()
                };

                var payload = new CreateStepHistoryRequestDto
                {
                    StepId = selectedDepartment.Id,
                    Users = departmentToMove.Users.Select(u => u.Id).ToList(),
                    FromDepartment = selectedDepartment.Name,
                    ToDepartment = departmentToMove.Name
                };

                await stepService.UpdateStepAsync(departmentToMove.Id, targetPayload);

                Console.WriteLine($"Target payload: {targetPayload.UserIds.Count} users");
                Console.WriteLine($"Source payload: {sourcePayload.UserIds.Count} users");

                await stepHistoryService.CreateStepHistoryForMoveUsersAsync(payload);

                await RefreshAllData();
                await CloseMoveModal();

                Console.WriteLine("✅ Users moved successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 Error moving users: {ex.Message}");
                error = "Unexpected error occurred while moving users.";
            }
        }

        private void MoveEditUserToTarget(UserResponseDto user)
        {
            if (departmentToMove == null || selectedDepartment == null)
                return;

            selectedDepartment.Users?.Remove(user);

            departmentToMove.Users ??= new List<UserResponseDto>();
            departmentToMove.Users.Add(user);

            StateHasChanged();
        }

        private void MoveEditTeamToTarget(TeamResponseDto team)
        {
            if (departmentToMove == null || selectedDepartment == null)
                return;

            selectedDepartment.Teams?.Remove(team);

            departmentToMove.Teams ??= new List<TeamResponseDto>();
            departmentToMove.Teams.Add(team);

            var teamUsers = team.Users ?? new List<UserResponseDto>();
            departmentToMove.Users ??= new List<UserResponseDto>();

            foreach (var user in teamUsers)
            {
                if (!departmentToMove.Users.Any(u => u.Id == user.Id))
                {
                    departmentToMove.Users.Add(user);
                }
            }

            StateHasChanged();
        }

        private void ToggleEditDropdown()
        {
            isEditDropdownOpen = !isEditDropdownOpen;
        }
    }
}