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

namespace FlowManager.Client.Components.Admin.Steps
{
    public partial class Steps: ComponentBase
    {
        private List<StepResponseDto> departments = new();
        private bool isModalOpen = false;
        private bool isCreateModalOpen = false;
        private StepResponseDto? selectedDepartment;
        private string newDepName = string.Empty;
        private string error = string.Empty;
        private List<UserResponseDto> unsignedUsers = new();
        private List<UserResponseDto> selectedUsers = new();
        private bool isEditModalOpen = false;
        private string editDepName = string.Empty;
        private List<UserResponseDto> allUsersList = new();
        private List<UserResponseDto> allUsers = new();


        [Inject]
        private StepService stepService { get; set; } = default!;

        [Inject]
        private UserService userService { get; set; } = default!;

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
                    departments = response.Result.Data.Where(d => d.DeletedAt == null).ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading departments: {ex.Message}");
            }
        }

        private async Task LoadUnsignedUsers()
        {
            try
            {
                unsignedUsers.Clear();
                var response = await userService.GetAllUsersQueriedAsync();
                if (response != null && response.Success && response.Result != null)
                {
                    var allUsers = (List<UserResponseDto>)response.Result.Data;
                    foreach(var user in allUsers)
                    {
                        var isAssigned = await userService.VerifyIfAssigned(user.Id);
                        if(isAssigned.Success && !isAssigned.Result)
                        {
                            unsignedUsers.Add(user);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading unsigned users: {ex.Message}");
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
            await LoadUnsignedUsers();
            newDepName = string.Empty;
            error = string.Empty;
            isCreateModalOpen = true;
            selectedUsers.Clear();
            StateHasChanged();
        }

        private void CloseCreateModal()
        {
            isCreateModalOpen = false;
            error = string.Empty;
            newDepName = string.Empty;
            selectedUsers.Clear();
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
                    Users = new List<StepUser>(),
                    Teams = new List<StepTeam>()
                };

                var result = await stepService.CreateStepAsync(step);

                if (result != null)
                {
                    await LoadDepartments();

                    foreach (var user in selectedUsers)
                    {
                        var assignResult = await stepService.AssignUserToStepAsync(result.Id, user.Id);
                        if (assignResult == null)
                        {
                            Console.WriteLine($"⚠️ Failed to assign user {user.Name} to department {result.Id}");
                            error = $"Failed to assign user {user.Name}.";
                        }
                    }

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
                await LoadUnsignedUsers();

                StateHasChanged();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error refreshing data: {ex.Message}");
            }
        }

        private async Task DeleteDepartment(Guid departmentId)
        {
            try
            {
                var response = await stepService.DeleteStepAsync(departmentId);
                if (response)
                {
                    var departmentToRemove = departments.FirstOrDefault(d => d.Id == departmentId);

                    if (departmentToRemove != null)
                    {
                        foreach (var user in departmentToRemove.Users)
                        {
                            await stepService.UnassignUserFromStepAsync(departmentId, user.Id);
                        }

                        departments.Remove(departmentToRemove);
                    }
                    await LoadDepartments();
                    isModalOpen = false;
                    selectedDepartment = null;
                    StateHasChanged();
                    Console.WriteLine("✅ Department deleted successfully!");
                }
                else
                {
                    Console.WriteLine("❌ Failed to delete department");
                    error = "Could not delete department.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 Error deleting step: {ex.Message}");
                error = "Unexpected error occurred.";
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
            var colors = new string[]
            {
        "#f87171", "#fbbf24", "#34d399", "#60a5fa", "#a78bfa", "#f472b6", "#facc15"
            };

            var rnd = new Random();
            var c1 = colors[rnd.Next(colors.Length)];
            var c2 = colors[rnd.Next(colors.Length)];

            return $"linear-gradient(135deg, {c1}, {c2})";
        }
    }
}