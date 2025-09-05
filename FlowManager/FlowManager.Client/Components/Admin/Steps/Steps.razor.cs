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
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Linq.Expressions;
using FlowManager.Shared.DTOs.Requests.Step;
using Microsoft.AspNetCore.Components.Web;

namespace FlowManager.Client.Components.Admin.Steps
{
    public partial class Steps : ComponentBase
    {
        private List<StepResponseDto> departments = new();
        private bool isModalOpen = false;
        private bool isCreateModalOpen = false;
        private bool isDeleteModalOpen = false;
        private bool isMoveUsersModalOpen = false;
        private Guid selectedReassignDepartmentId { get; set; }
        private StepResponseDto? selectedDepartment;
        private StepResponseDto? departmentToDelete;
        private string newDepName = string.Empty;
        private string error = string.Empty;
        private List<UserResponseDto> unsignedUsers = new();
        private List<UserResponseDto> selectedUsers = new();
        private bool isEditModalOpen = false;
        private string editDepName = string.Empty;
        private List<UserResponseDto> allUsersList = new();
        private List<UserResponseDto> allUsers = new();
        private Dictionary<Guid, string> departmentColors = new();
        private Guid? draggedUserId = null;


        [Inject]
        private StepService stepService { get; set; } = default!;

        [Inject]
        private UserService userService { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            await LoadDepartments();

            if (departments != null)
            {
                foreach (var dep in departments)
                {
                    if (!departmentColors.ContainsKey(dep.Id))
                    {
                        departmentColors[dep.Id] = GetRandomGradient();
                    }
                }
            }
        }

        private async Task LoadDepartments()
        {
            try
            {
                var response = await stepService.GetStepsQueriedAsync();
                if (response != null && response.Success && response.Result != null)
                {
                    departments = response.Result.Data.Where(d => d.DeletedAt == null).ToList();
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
        }

        private void CloseModal()
        {
            isModalOpen = false;
            selectedDepartment = null;
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

        private async Task OpenDeleteDepartmentModal(Guid departmentId)
        {
            var step = await stepService.GetStepAsync(departmentId);

            if (step != null)
            {
                departmentToDelete = step;
                isDeleteModalOpen = true;
                CloseModal();
                CloseEditModal();
                StateHasChanged();
            }
        }

        private void CloseDeleteDepartmentModal()
        {
            isDeleteModalOpen = false;
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
                CloseDeleteDepartmentModal();
                StateHasChanged();
            }
        }


        private async Task OpenEditModal(StepResponseDto department)
        {
            var stepDetails = await stepService.GetStepAsync(department.Id);

            if (stepDetails != null)
            {
                selectedDepartment = stepDetails;
                editDepName = stepDetails.Name;
                selectedUsers = stepDetails.Users.ToList();

                var response = await userService.GetAllUsersQueriedAsync();
                if (response != null && response.Success && response.Result != null)
                {
                    allUsersList = ((List<UserResponseDto>)response.Result.Data);

                    var filteredUsers = new List<UserResponseDto>();

                    foreach (var u in allUsersList)
                    {
                        var isAssigned = await userService.VerifyIfAssigned(u.Id);

                        if ((isAssigned.Success && !isAssigned.Result) || selectedDepartment.Users.Any(su => su.Id == u.Id))
                        {
                            filteredUsers.Add(u);
                        }
                    }

                    allUsers = filteredUsers;

                }

                isEditModalOpen = true;
                StateHasChanged();
            }
        }

        private async Task SaveEditDepartment()
        {
            if (selectedDepartment == null)
                return;

            try
            {
                selectedDepartment = await stepService.GetStepAsync(selectedDepartment.Id);
                Console.WriteLine($"{selectedDepartment.Id}");
                selectedUsers ??= new List<UserResponseDto>();

                var currentUserIds = (selectedDepartment?.Users ?? new List<UserResponseDto>())
                        .Select(u => u.Id)
                        .ToList();

                var newUserIds = (selectedUsers ?? new List<UserResponseDto>())
                                        .Select(u => u.Id)
                                        .ToList();

                var usersToAssign = newUserIds.Except(currentUserIds).ToList();

                var usersToUnassign = currentUserIds.Except(newUserIds).ToList();

                foreach (var userId in usersToAssign)
                {
                    var updatedStep = await stepService.AssignUserToStepAsync(selectedDepartment.Id, userId);
                    Console.WriteLine($"{userId} assigned to {selectedDepartment.Id}");
                    if (updatedStep != null)
                        selectedDepartment = updatedStep;
                }

                Console.WriteLine($"Users to unassign: {string.Join(", ", usersToUnassign)}");
                foreach (var userId in usersToUnassign)
                {
                    var updatedStep = await stepService.UnassignUserFromStepAsync(selectedDepartment.Id, userId);
                    Console.WriteLine($"{userId} unassigned from {selectedDepartment.Id}");
                    if (updatedStep != null)
                        selectedDepartment = updatedStep;
                }

                selectedUsers = selectedDepartment.Users.ToList();

                await RefreshAllData();
                await LoadDepartments();

                StateHasChanged();

                CloseEditModal();
                Console.WriteLine("✅ Department updated successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 Error updating department: {ex.Message}");
                error = "Unexpected error occurred.";
            }
        }

        private void CloseEditModal()
        {
            isEditModalOpen = false;
            selectedDepartment = null;
            editDepName = string.Empty;
            selectedUsers.Clear();
            allUsers.Clear();
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
                    await stepService.DeleteStepAsync(departmentToDelete.Id);
                    departments.RemoveAll(d => d.Id == departmentToDelete.Id);
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
                await OpenDeleteDepartmentModal(departmentToDelete.Id);
                StateHasChanged();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eroare la salvarea mutărilor: {ex.Message}");
            }
        }

        private async Task CloseMoveUsersModal()
        {
            isMoveUsersModalOpen = false;
            selectedDepartment = null;
            selectedReassignDepartmentId = Guid.Empty;

            StateHasChanged();
        }
    }
}