using FlowManager.Client.DTOs;
using FlowManager.Client.Services;
using FlowManager.Client.ViewModels;
using FlowManager.Client.ViewModels.Team;
using FlowManager.Domain.Entities;
using FlowManager.Shared.DTOs.Requests.Flow;
using FlowManager.Shared.DTOs.Requests.FlowStep;
using FlowManager.Shared.DTOs.Requests.FlowStepItem;
using FlowManager.Shared.DTOs.Requests.FlowStepItemTeam;
using FlowManager.Shared.DTOs.Responses;
using FlowManager.Shared.DTOs.Responses.Flow;
using FlowManager.Shared.DTOs.Responses.FlowStepItemUser;
using FlowManager.Shared.DTOs.Responses.Step;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace FlowManager.Client.Components.Admin.EditFlow
{
    public partial class EditFlow : ComponentBase
    {
        [Inject] private StepService _stepService { get; set; } = default!;
        [Inject] private FlowService _flowService { get; set; } = default!;

        [Parameter] public Guid? FormTemplateId { get; set; }
        [Parameter] public Guid FlowId { get; set; }
        [Parameter] public EventCallback OnFlowSaved { get; set; }

        private List<StepVM> _availableSteps = new List<StepVM>();
        private List<FlowStepVM> _configuredSteps = new List<FlowStepVM>();
        private StepVM? _draggedStep = null;
        private bool _isDragOver = false;
        private string _flowName = string.Empty;
        private string _initialFlowName = string.Empty;

        private bool _showAssignToStepModal = false;
        private FlowStepItemVM? _flowStepToAssign = null;
        private int _flowStepToAssignIndex = 0;
        private int _flowStepItemToAssignIndex = 0;

        private string _onSubmitMessage = string.Empty;
        private bool _onSubmitSuccess;

        protected override async Task OnInitializedAsync()
        {
            await LoadCurrentFlow();
            await LoadStepsAsync();
        }

        private async Task LoadCurrentFlow()
        {
            ApiResponse<FlowResponseDto?> response = await _flowService.GetFlowByIdIncludeStepsAsync(FlowId);
            if (!response.Success || response.Result == null)
            {
                _onSubmitMessage = response?.Message ?? "Failed to load flow";
                _onSubmitSuccess = false;
                _configuredSteps.Clear();
                return;
            }

            _flowName = response.Result.Name ?? string.Empty;
            _initialFlowName = response.Result.Name ?? string.Empty;

            // Convert the loaded flow to the new FlowStep/FlowStepItem structure
            _configuredSteps = response.Result.FlowSteps?.Select(fs => new FlowStepVM
            {
                Id = fs.Id,
                FlowId = fs.FlowId,
                FlowStepItems = fs.FlowStepItems.Select(flowStepItem => new FlowStepItemVM
                {
                    Id = flowStepItem.Id,
                    FlowStepId = flowStepItem.FlowStepId,
                    StepId = flowStepItem.StepId,
                    Step = new StepVM
                    {
                        Id = flowStepItem.Step?.StepId,
                        Name = flowStepItem.Step?.StepName,
                    },
                    AssignedUsers = flowStepItem.AssignedUsers?.Select(au => new FlowStepItemUserVM
                    {
                        FlowStepItemId = au.FlowStepItemId,
                        UserId = au.UserId ?? Guid.Empty,
                        User = new UserVM
                        {
                            Id = au.User?.Id ?? Guid.Empty,
                            Name = au.User?.Name,
                            Email = au.User?.Email,
                        },
                    }).ToList() ?? new List<FlowStepItemUserVM>(),
                    AssignedTeams = flowStepItem.AssignedTeams?.Select(at => new FlowStepItemTeamVM
                    {
                        FlowStepItemId = at.FlowStepItemId,
                        TeamId = at.TeamId,
                        Team = new TeamVM
                        {
                            Id = at.Team?.Id ?? Guid.Empty,
                            Name = at.Team?.Name,
                            Users = at.Team?.Users?.Select(user => new UserVM
                            {
                                Id = user.Id,
                                Name = user.Name,
                                Email = user.Email,
                            }).ToList() ?? new List<UserVM>(),
                        },
                    }).ToList() ?? new List<FlowStepItemTeamVM>(),
                }).ToList(),
            }).ToList() ?? new List<FlowStepVM>();
        }

        private async Task LoadStepsAsync()
        {
            ApiResponse<PagedResponseDto<StepResponseDto>> response = await _stepService.GetAllStepsIncludeUsersAndTeamsQueriedAsync();
            if (!response.Success)
            {
                return;
            }

            _availableSteps = response.Result.Data
                .Select(step => new StepVM
                {
                    Id = step.StepId,
                    Name = step.StepName,
                    Users = step.Users?.Select(u => new UserVM
                    {
                        Id = u.Id,
                        Name = u.Name,
                        Email = u.Email,
                    }).ToList() ?? new List<UserVM>(),
                    Teams = step.Teams?.Select(t => new TeamVM
                    {
                        Id = t.Id,
                        Name = t.Name,
                        Users = t.Users?.Select(user => new UserVM
                        {
                            Id = user.Id,
                            Name = user.Name,
                            Email = user.Email,
                        }).ToList() ?? new List<UserVM>(),
                    }).ToList() ?? new List<TeamVM>(),
                }).ToList();
        }

        private void HandleDragStart(DragEventArgs e, StepVM step)
        {
            _draggedStep = step;
            e.DataTransfer.EffectAllowed = "copy";
            StateHasChanged();
        }

        private void HandleDragEnd(DragEventArgs e)
        {
            _draggedStep = null;
            _isDragOver = false;
            StateHasChanged();
        }

        private void HandleDragEnter(DragEventArgs e)
        {
            if (_draggedStep != null)
            {
                _isDragOver = true;
                StateHasChanged();
            }
        }

        private void HandleDragLeave(DragEventArgs e)
        {
            _isDragOver = false;
            StateHasChanged();
        }

        private void HandleDragOver(DragEventArgs e)
        {
            e.DataTransfer.DropEffect = "copy";
        }

        private void HandleDrop(DragEventArgs e)
        {
            _isDragOver = false;
            if (_draggedStep != null)
            {
                // Create a new FlowStep with an empty FlowStepItem
                var stepForWorkflow = new FlowStepVM
                {
                    Id = Guid.NewGuid(),
                    FlowStepItems = new List<FlowStepItemVM>()
                };
                _configuredSteps.Add(stepForWorkflow);
            }
            _draggedStep = null;
            StateHasChanged();
        }

        private void AddFlowStepItem(int flowStepIndex)
        {
            if (flowStepIndex >= 0 && flowStepIndex < _configuredSteps.Count)
            {
                var flowStep = _configuredSteps[flowStepIndex];

                // Create a new FlowStepItem with no assigned step initially
                var newFlowStepItem = new FlowStepItemVM
                {
                    Id = Guid.NewGuid(),
                    FlowStepId = flowStep.Id,
                    FlowStep = flowStep,
                    AssignedUsers = new List<FlowStepItemUserVM>(),
                    AssignedTeams = new List<FlowStepItemTeamVM>()
                };

                flowStep.FlowStepItems.Add(newFlowStepItem);

                // Open the assignment modal immediately
                ShowAssingToStepModal(newFlowStepItem, flowStepIndex, flowStep.FlowStepItems.Count - 1);

                StateHasChanged();
            }
        }

        private void RemoveFlowStepItem(int flowStepIndex, int flowStepItemIndex, MouseEventArgs e)
        {
            if (flowStepIndex >= 0 && flowStepIndex < _configuredSteps.Count)
            {
                var flowStep = _configuredSteps[flowStepIndex];
                if (flowStepItemIndex >= 0 && flowStepItemIndex < flowStep.FlowStepItems.Count)
                {
                    flowStep.FlowStepItems.RemoveAt(flowStepItemIndex);
                    StateHasChanged();
                }
            }
        }

        private void MoveFlowStepItemUp(int flowStepIndex, int flowStepItemIndex, MouseEventArgs e)
        {
            if (flowStepIndex >= 0 && flowStepIndex < _configuredSteps.Count)
            {
                var flowStep = _configuredSteps[flowStepIndex];
                if (flowStepItemIndex > 0 && flowStepItemIndex < flowStep.FlowStepItems.Count)
                {
                    var item = flowStep.FlowStepItems[flowStepItemIndex];
                    flowStep.FlowStepItems.RemoveAt(flowStepItemIndex);
                    flowStep.FlowStepItems.Insert(flowStepItemIndex - 1, item);
                    StateHasChanged();
                }
            }
        }

        private void MoveFlowStepItemDown(int flowStepIndex, int flowStepItemIndex, MouseEventArgs e)
        {
            if (flowStepIndex >= 0 && flowStepIndex < _configuredSteps.Count)
            {
                var flowStep = _configuredSteps[flowStepIndex];
                if (flowStepItemIndex >= 0 && flowStepItemIndex < flowStep.FlowStepItems.Count - 1)
                {
                    var item = flowStep.FlowStepItems[flowStepItemIndex];
                    flowStep.FlowStepItems.RemoveAt(flowStepItemIndex);
                    flowStep.FlowStepItems.Insert(flowStepItemIndex + 1, item);
                    StateHasChanged();
                }
            }
        }

        private void RemoveConfiguredStep(int index)
        {
            _configuredSteps.RemoveAt(index);

            // Reorder remaining FlowSteps
            for (int i = 0; i < _configuredSteps.Count; i++)
            {
                _configuredSteps[i].Order = i + 1;
            }

            StateHasChanged();
        }

        public List<FlowStepVM> GetConfiguredWorkflow()
        {
            return _configuredSteps.ToList();
        }

        public List<Guid> GetConfiguredStepIds()
        {
            return _configuredSteps
                .SelectMany(fs => fs.FlowStepItems)
                .Where(fsi => fsi.StepId.HasValue)
                .Select(fsi => fsi.StepId.Value)
                .ToList();
        }

        public void ClearConfiguration()
        {
            _configuredSteps.Clear();
            _flowName = string.Empty;
            StateHasChanged();
        }

        private async Task SaveCurrentWorkflowAsync()
        {
            PostFlowRequestDto payload = new PostFlowRequestDto
            {
                FormTemplateId = FormTemplateId,
                Name = _flowName,
                FlowSteps = _configuredSteps.Select(configuredStep => new PostFlowStepRequestDto
                {
                    FlowStepItems = configuredStep.FlowStepItems.Select(fsi => new PostFlowStepItemRequestDto
                    {
                        StepId = fsi.StepId ?? Guid.Empty,
                        FlowStepId = fsi.FlowStepId ?? Guid.Empty,
                        AssignedUsersIds = fsi.AssignedUsers?.Select(au => au.UserId ?? Guid.Empty).ToList() ?? new List<Guid>(),
                        AssignedTeams = fsi.AssignedTeams?.Select(at => new PostFlowTeamRequestDto
                        {
                            TeamId = at.TeamId ?? Guid.Empty,
                            UserIds = at.Team?.Users?.Select(au => au.Id).ToList() ?? new List<Guid>(),
                        }).ToList() ?? new List<PostFlowTeamRequestDto>(),
                    }).ToList(),
                }).ToList(),
            };

            ApiResponse<FlowResponseDto> response = await _flowService.PostFlowAsync(payload);

            _onSubmitMessage = response.Message;
            _onSubmitSuccess = response.Success;

            StateHasChanged();

            if (response.Success)
            {
                await Task.Delay(3000);
                _onSubmitMessage = string.Empty;
                await OnFlowSaved.InvokeAsync(null);
            }
        }

        public void MoveStepUp(int index)
        {
            if (index > 0 && index < _configuredSteps.Count)
            {
                var step = _configuredSteps[index];
                _configuredSteps.RemoveAt(index);
                _configuredSteps.Insert(index - 1, step);

                // Update order numbers
                for (int i = 0; i < _configuredSteps.Count; i++)
                {
                    _configuredSteps[i].Order = i + 1;
                }

                StateHasChanged();
            }
        }

        public void MoveStepDown(int index)
        {
            if (index >= 0 && index < _configuredSteps.Count - 1)
            {
                var step = _configuredSteps[index];
                _configuredSteps.RemoveAt(index);
                _configuredSteps.Insert(index + 1, step);

                // Update order numbers
                for (int i = 0; i < _configuredSteps.Count; i++)
                {
                    _configuredSteps[i].Order = i + 1;
                }

                StateHasChanged();
            }
        }

        public int GetTotalUsersInWorkflow()
        {
            return _configuredSteps.Sum(step =>
                step.FlowStepItems.Sum(flowStepItem =>
                    (flowStepItem.AssignedUsers?.Count ?? 0) +
                    (flowStepItem.AssignedTeams?.Count ?? 0)
                ));
        }

        public bool IsWorkflowValid()
        {
            return !string.IsNullOrWhiteSpace(_flowName) &&
                   _flowName.ToUpper() != _initialFlowName.ToUpper() &&
                   _configuredSteps.Any() &&
                   _configuredSteps.All(fs => fs.FlowStepItems.Any() &&
                        fs.FlowStepItems.All(fsi =>
                            (fsi.AssignedUsers?.Any() ?? false) ||
                            (fsi.AssignedTeams?.Any() ?? false)
                        ));
        }

        public string GetFlowNameValidationClass()
        {
            if (string.IsNullOrEmpty(_flowName))
                return "";

            return string.IsNullOrWhiteSpace(_flowName) ? "invalid" : "valid";
        }

        private void ShowAssingToStepModal(FlowStepItemVM flowStepItem, int flowStepIndex, int flowStepItemIndex)
        {
            _showAssignToStepModal = true;
            _flowStepToAssign = flowStepItem;
            _flowStepToAssignIndex = flowStepIndex;
            _flowStepItemToAssignIndex = flowStepItemIndex;
            StateHasChanged();
        }

        private void ConfigureStepsToFlow()
        {
            if (_flowStepToAssignIndex >= 0 && _flowStepToAssignIndex < _configuredSteps.Count &&
                _flowStepItemToAssignIndex >= 0 && _flowStepItemToAssignIndex < _configuredSteps[_flowStepToAssignIndex].FlowStepItems.Count)
            {
                FlowStepItemVM flowStepItem = _configuredSteps[_flowStepToAssignIndex].FlowStepItems[_flowStepItemToAssignIndex];

                if (_flowStepToAssign != null)
                {
                    flowStepItem.AssignedUsers = _flowStepToAssign.AssignedUsers ?? new List<FlowStepItemUserVM>();
                    flowStepItem.AssignedTeams = _flowStepToAssign.AssignedTeams ?? new List<FlowStepItemTeamVM>();
                    flowStepItem.Step = _flowStepToAssign.Step;
                    flowStepItem.StepId = _flowStepToAssign.StepId;
                }

                StateHasChanged();
            }
        }
    }
}