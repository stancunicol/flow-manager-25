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
using FlowManager.Domain.Dtos;
using Microsoft.AspNetCore.Identity;
using FlowManager.Shared.DTOs.Requests;
using MudBlazor;
using QueryParams = FlowManager.Shared.DTOs.Requests.QueryParamsDto;
using Azure;

namespace FlowManager.Client.Components.Admin.Steps
{
    public partial class Steps : ComponentBase
    {
        private List<StepResponseDto> departments = new();
        private List<StepResponseDto> departmentsInUI = new();
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
        private int pageSize = 12;
        private int currentPage = 1;
        private bool hasMoreDepartments = false;
        private string searchTerm = string.Empty;
        private System.Timers.Timer? debounceTimer;

        [Inject]
        private StepService stepService { get; set; } = default!;
        [Inject]
        private UserService userService { get; set; } = default!;
        [Inject]
        private TeamService teamService { get; set; } = default!;
        [Inject]
        private StepHistoryService stepHistoryService { get; set; } = default!;
        [Inject]
        private NavigationManager Navigation { get; set; }
        [Parameter]
        public EventCallback<string> OnTabChange { get; set; }

        private async Task GoToHistory()
        {
            if (OnTabChange.HasDelegate)
            {
                await OnTabChange.InvokeAsync("STEPS_HISTORY");
            }
        }

        protected override async Task OnInitializedAsync()
        {
            await LoadDepartments(currentPage);
            await LoadAllDepartments();
        }

        private async Task LoadAllDepartments()
        {
            var result = await stepService.GetStepsQueriedAsync();
            if (result != null)
                departments = result.Result.Data
                .Where(d => d.DeletedAt == null)
                .ToList();

        }

        private async Task LoadDepartments(int page)
        {
            try
            {
                var payload = new QueriedStepRequestDto
                {
                    QueryParams = new QueryParams
                    {
                        Page = page,
                        PageSize = pageSize
                    }
                };

                var response = await stepService.GetStepsQueriedAsync(payload);

                if (response != null && response.Success && response.Result != null)
                {
                    var pageDepartments = response.Result.Data
                        .Where(d => d.DeletedAt == null)
                        .ToList();

                    for (int i = 0; i < pageDepartments.Count; i++)
                    {
                        var departmentDetails = await stepService.GetStepAsync(pageDepartments[i].Id);
                        if (departmentDetails != null)
                            pageDepartments[i] = departmentDetails;

                        pageDepartments[i].Users ??= new List<UserResponseDto>();
                        pageDepartments[i].Teams ??= new List<TeamResponseDto>();

                        if (!departmentColors.ContainsKey(pageDepartments[i].Id))
                            departmentColors[pageDepartments[i].Id] = GetRandomGradient();
                    }

                    departmentsInUI.AddRange(pageDepartments);
                    currentPage++;
                    hasMoreDepartments = pageDepartments.Count == pageSize;
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

                    await LoadDepartments(currentPage);

                    await LoadAllDepartments();

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
                departmentsInUI.Clear();
                currentPage = 1;
                unsignedUsers.Clear();
                hasMoreDepartments = false;

                await LoadDepartments(currentPage);
                await LoadAllDepartments();

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
            }

                isDeleteModalOpen = true;
                CloseModal();
                CloseEditModal();
                StateHasChanged();
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
            if (!selectedDepartment.Users.Any(u => u.Id == user.Id))
            {
                selectedDepartment.Users.Add(user);
            }

            StateHasChanged();
        }

        private void MoveTeamToTarget(TeamResponseDto team)
        {
            if (departmentToDelete == null || selectedDepartment == null)
                return;

            departmentToDelete.Teams?.Remove(team);

            selectedDepartment.Teams ??= new List<TeamResponseDto>();
            if (!selectedDepartment.Teams.Any(t => t.Id == team.Id))
            {
                selectedDepartment.Teams.Add(team);
            }

            StateHasChanged();
        }

        private async Task SaveUserMoves()
        {
            try
            {
                var targetPayload = new PatchStepRequestDto
                {
                    UserIds = selectedDepartment.Users.Select(u => u.Id).ToList(),
                    TeamIds = selectedDepartment.Teams.Select(t => t.Id).ToList()
                };

                await stepService.UpdateStepAsync(selectedDepartment.Id, targetPayload);

                var deletePayload = new PatchStepRequestDto
                {
                    UserIds = departmentToDelete.Users?.Select(u => u.Id).ToList() ?? new List<Guid>(),
                    TeamIds = departmentToDelete.Teams?.Select(t => t.Id).ToList() ?? new List<Guid>()
                };

                await stepService.UpdateStepAsync(departmentToDelete.Id, deletePayload);

                bool isEmpty = (departmentToDelete.Users == null || !departmentToDelete.Users.Any())
                               && (departmentToDelete.Teams == null || !departmentToDelete.Teams.Any());

                if (isEmpty)
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
                        departmentsInUI.RemoveAll(d => d.Id == departmentToDelete.Id);
                        departments.RemoveAll(d => d.Id == departmentToDelete.Id);
                    }

                    await CloseMoveUsersModal();
                    StateHasChanged();
                }
                else
                {
                    await CloseMoveUsersModal();
                    isDeleteModalOpen = true;
                    StateHasChanged();

                    Console.WriteLine($"♻️ Department '{departmentToDelete.Name}' still has users/teams, reopening selection.");
                }
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

            selectedDepartment.Users ??= new List<UserResponseDto>();
            selectedDepartment.Teams ??= new List<TeamResponseDto>();

            foreach (var user in selectedDepartment.Users)
            {
                user.Teams ??= new List<TeamResponseDto>();
            }

            isChangeUsersModalOpen = true;
            StateHasChanged();
        }

        private async Task OpenMoveModal()
        {
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
                    UserIds = selectedDepartment.Users.Select(u => u.Id).ToList(),
                    TeamIds = selectedDepartment.Teams.Select(t => t.Id).ToList()
                };

                await stepService.UpdateStepAsync(selectedDepartment.Id, sourcePayload);

                var targetPayload = new PatchStepRequestDto
                {
                    Name = departmentToMove.Name,
                    UserIds = departmentToMove.Users.Select(u => u.Id).ToList(),
                    TeamIds = departmentToMove.Teams.Select(t => t.Id).ToList()
                };

                var movedUsers = departmentToMove.Users
                .Concat(departmentToMove.Teams.SelectMany(t => t.Users))
                .Where(u => !selectedDepartment.Users.Any(su => su.Id == u.Id) &&
                            !selectedDepartment.Teams.SelectMany(t => t.Users).Any(su => su.Id == u.Id))
                .Select(u => u.Name)
                .Distinct()
                .ToList();

                var payload = new CreateStepHistoryRequestDto
                {
                    StepId = selectedDepartment.Id,
                    Users = movedUsers,
                    FromDepartment = selectedDepartment.Name,
                    ToDepartment = departmentToMove.Name
                };

                Console.WriteLine($"History payload users: {string.Join(", ", payload.Users)}");

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
            if (!departmentToMove.Users.Any(u => u.Id == user.Id))
            {
                departmentToMove.Users.Add(user);
            }

            StateHasChanged();
        }


        private async Task MoveEditTeamToTargetAsync(TeamResponseDto team)
        {
            if (departmentToMove == null || selectedDepartment == null)
                return;

            selectedDepartment.Teams?.Remove(team);

            departmentToMove.Teams ??= new List<TeamResponseDto>();
            if (!departmentToMove.Teams.Any(t => t.Id == team.Id))
            {
                departmentToMove.Teams.Add(team);
            }

            StateHasChanged();
        }


        private void ToggleEditDropdown()
        {
            isEditDropdownOpen = !isEditDropdownOpen;
        }

        private void OnSearchChanged(ChangeEventArgs e)
        {
            searchTerm = e.Value?.ToString() ?? string.Empty;

            debounceTimer?.Stop();
            debounceTimer = new System.Timers.Timer(300);
            debounceTimer.Elapsed += async (sender, args) =>
            {
                debounceTimer?.Stop();
                await InvokeAsync(ApplySearchFilter);
            };
            debounceTimer.Start();
        }

        private Task ApplySearchFilter()
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                departmentsInUI = departments.Take(pageSize * currentPage).ToList();
            }
            else
            {
                departmentsInUI = departments
                    .Where(d => d.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            StateHasChanged();
            return Task.CompletedTask;
        }

        private string GetDepartmentColor(Guid departmentId)
        {
            if (!departmentColors.ContainsKey(departmentId))
            {
                departmentColors[departmentId] = GetRandomGradient();
            }
            return departmentColors[departmentId];
        }

        private List<UserResponseDto> GetIndividualUsers(StepResponseDto department)
        {
            if (department?.Users == null) return new List<UserResponseDto>();

            var usersInTeams = new HashSet<Guid>();
            if (department.Teams != null)
            {
                foreach (var team in department.Teams)
                {
                    if (team.Users != null)
                    {
                        foreach (var teamUser in team.Users)
                        {
                            usersInTeams.Add(teamUser.Id);
                        }
                    }
                }
            }

            return department.Users
                .Where(user => !usersInTeams.Contains(user.Id))
                .ToList();
        }
    }
}