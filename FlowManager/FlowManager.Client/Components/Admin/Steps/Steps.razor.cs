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

namespace FlowManager.Client.Components.Admin.Steps
{
    public partial class Steps: ComponentBase
    {
        private List<StepResponseDto> departments = new();
        private bool isModalOpen = false;
        private bool isCreateModalOpen = false;
        private StepResponseDto? selectedDepartment;
        private string newDepName = string.Empty;
        private string error;
        private List<UserResponseDto> unsignedUsers = new();
        private List<UserResponseDto> selectedUsers = new();

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
                    departments = (List<StepResponseDto>)response.Result.Data;
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
                    FlowSteps = new List<FlowStep>(),
                    Users = new List<StepUser>(),
                    Teams = new List<StepTeam>()
                };

                var result = await stepService.CreateStepAsync(step);

                if (result != null)
                {
                    departments.Add(new StepResponseDto { Id = result.Id, Name = newDepName.Trim() });

                    foreach (var user in selectedUsers)
                    {
                        var assignResult = await userService.AssignUserToStepAsync(result.Id, user.Id);
                        if (!assignResult)
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
                selectedUsers.Add(user);
            else
                selectedUsers.Remove(user);
        }

        private async Task RefreshAllData()
        {
            try
            {
                departments.Clear();
                unsignedUsers.Clear();
                selectedUsers.Clear();

                await LoadDepartments();
                await LoadUnsignedUsers();

                StateHasChanged();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error refreshing data: {ex.Message}");
            }
        }
    }
}