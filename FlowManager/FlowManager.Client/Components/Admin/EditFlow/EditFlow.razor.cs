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
            _configuredSteps = response.Result.FlowSteps?.Select(fs => new FlowStepVM
            {
                FlowStepItems = fs.FlowStepItems.Select(flowStepItem => new FlowStepItemVM
                {
                    FlowStepId = flowStepItem.FlowStepId,
                    StepId = flowStepItem.StepId,
                    Step = new StepVM
                    {
                        Id = flowStepItem.Step?.StepId,
                        Name = flowStepItem.Step?.StepName,
                    },
                    AssignedUsers = flowStepItem.AssignedUsers?.Select(au => new FlowStepItemUserVM
                    {
                        UserId = au.UserId ?? Guid.Empty,
                        User = new UserVM
                        {
                            Id = au.User?.Id ?? Guid.Empty,
                            Name = au.User?.Name,
                            Email = au.User?.Email,
                        },
                    }).ToList() ?? new List<FlowStepItemUserVM>(),
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
                var stepForWorkflow = new FlowStepVM
                {
                    Id = _draggedStep.Id,
                };
                _configuredSteps.Add(stepForWorkflow);
            }
            _draggedStep = null;
            StateHasChanged();
        }

        private void RemoveConfiguredStep(int index)
        {
            _configuredSteps.RemoveAt(index);
            StateHasChanged();
        }

        public List<FlowStepVM> GetConfiguredWorkflow()
        {
            return _configuredSteps.ToList();
        }

        public List<Guid> GetConfiguredStepIds()
        {
            return _configuredSteps.Select(s => s.Id ?? Guid.Empty).ToList();
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
                            UserIds = at.Team.Users?.Select(au => au.Id).ToList() ?? new List<Guid>(),
                        }).ToList() ?? new List<PostFlowTeamRequestDto>(),
                    }).ToList(),
                }).ToList(),
            };

            ApiResponse<FlowResponseDto> response = await _flowService.PostFlowAsync(payload);

            _onSubmitMessage = response.Message;
            _onSubmitSuccess = response.Success;

            StateHasChanged();

            if(response.Success)
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
                StateHasChanged();
            }
        }

        public int GetTotalUsersInWorkflow()
        {
            return _configuredSteps.Sum(step =>
                step.FlowStepItems.SelectMany(flowStepItem => flowStepItem.AssignedUsers).Count() + step.FlowStepItems.SelectMany(flowStepItem => flowStepItem.AssignedTeams).Count());
        }

        public bool IsWorkflowValid()
        {
            return !string.IsNullOrWhiteSpace(_flowName) &&
                   _flowName.ToUpper() != _initialFlowName.ToUpper() &&
                   _configuredSteps.Any() &&
                   _configuredSteps.All(fs => fs.FlowStepItems.Any(flowStepItem => flowStepItem.AssignedTeams.Count > 0 || 
                        fs.FlowStepItems.Any(flowStepItem => flowStepItem.AssignedUsers.Count > 0)));
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
            FlowStepItemVM flowStepItem = _configuredSteps[_flowStepToAssignIndex].FlowStepItems[_flowStepItemToAssignIndex];

            flowStepItem.AssignedUsers = _flowStepToAssign!.AssignedUsers;

            flowStepItem.AssignedTeams = _flowStepToAssign!.AssignedTeams;

            StateHasChanged();
        }
    }
}