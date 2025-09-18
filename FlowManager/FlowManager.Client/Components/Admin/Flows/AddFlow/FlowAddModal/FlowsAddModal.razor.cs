using Azure;
using FlowManager.Client.DTOs;
using FlowManager.Client.Services;
using FlowManager.Client.ViewModels;
using FlowManager.Client.ViewModels.Team;
using FlowManager.Shared.DTOs.Requests.Flow;
using FlowManager.Shared.DTOs.Requests.FlowStep;
using FlowManager.Shared.DTOs.Requests.FlowStepItem;
using FlowManager.Shared.DTOs.Responses;
using FlowManager.Shared.DTOs.Responses.Flow;
using FlowManager.Shared.DTOs.Responses.Step;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Threading.Tasks;


namespace FlowManager.Client.Components.Admin.Flows.AddFlow.FlowAddModal
{
    public partial class FlowsAddModal : ComponentBase
    {
        [Inject] private StepService _stepService { get; set; } = default!;
        [Inject] private IJSRuntime _jsRuntime { get; set; } = default!;
        [Inject] private FlowService _flowService { get; set; } = default!;
        [Parameter] public string SavedFormTemplateName { get; set; } = "";
        [Parameter] public EventCallback OnSaveWorkflow { get; set; }
        [Parameter] public EventCallback OnFlowSavedWithoutTemplate { get; set; }

        private List<StepVM> _availableSteps = new List<StepVM>();
        private List<FlowStepVM> _configuredFlowSteps = new List<FlowStepVM>();
        private StepVM? _draggedStep = null;
        private bool _isDragOver = false;
        private int _isDragOverFlowStep = -1; // -1 means no flow step is being dragged over
        private string _flowName = string.Empty;

        private bool _showAssignToStepModal = false;
        private FlowStepItemVM? _flowStepItemToAssign = null;
        private int _flowStepToAssignIndex = 0;
        private int _flowStepItemToAssignIndex = 0;

        private string _onSubmitMessage = string.Empty;
        private bool _onSubmitSuccess;
        private bool _isSaving = false;

        protected override async Task OnInitializedAsync()
        {
            await LoadStepsAsync();
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

        private void AddFlowStep()
        {
            var newFlowStep = new FlowStepVM
            {
                Id = Guid.NewGuid(),
                FlowStepItems = new List<FlowStepItemVM>()
            };
            _configuredFlowSteps.Add(newFlowStep);
            StateHasChanged();
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
            _isDragOverFlowStep = -1;
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
            _isDragOverFlowStep = -1;

            _draggedStep = null;
            StateHasChanged();
        }

        private void HandleDragEnterFlowStep(DragEventArgs e, int flowStepIndex)
        {
            if (_draggedStep != null)
            {
                _isDragOverFlowStep = flowStepIndex;
                StateHasChanged();
            }
        }

        private void HandleDragLeaveFlowStep(DragEventArgs e, int flowStepIndex)
        {
            _isDragOverFlowStep = -1;
            StateHasChanged();
        }

        private void HandleDragOverFlowStep(DragEventArgs e, int flowStepIndex)
        {
            e.DataTransfer.DropEffect = "copy";
        }

        private void HandleDropOnFlowStep(DragEventArgs e, int flowStepIndex)
        {
            _isDragOver = false;
            _isDragOverFlowStep = -1;

            if (_draggedStep != null && flowStepIndex >= 0 && flowStepIndex < _configuredFlowSteps.Count)
            {
                var flowStep = _configuredFlowSteps[flowStepIndex];

                if (flowStep.FlowStepItems.Any(flowStepItem => flowStepItem.StepId == _draggedStep.Id))
                {
                    _draggedStep = null;
                    return;
                }

                var newFlowStepItem = new FlowStepItemVM
                {
                    Id = Guid.NewGuid(),
                    FlowStepId = flowStep.Id,
                    FlowStep = flowStep,
                    StepId = _draggedStep.Id,
                    Step = _draggedStep,
                    AssignedUsers = new List<FlowStepItemUserVM>(),
                    AssignedTeams = new List<FlowStepItemTeamVM>()
                };

                flowStep.FlowStepItems.Add(newFlowStepItem);

                ShowAssingToStepModal(newFlowStepItem, flowStepIndex, flowStep.FlowStepItems.Count - 1);
            }

            _draggedStep = null;
            StateHasChanged();
        }

        private void RemoveFlowStepItem(int flowStepIndex, int flowStepItemIndex, MouseEventArgs e)
        {
            if (flowStepIndex >= 0 && flowStepIndex < _configuredFlowSteps.Count)
            {
                var flowStep = _configuredFlowSteps[flowStepIndex];
                if (flowStepItemIndex >= 0 && flowStepItemIndex < flowStep.FlowStepItems.Count)
                {
                    flowStep.FlowStepItems.RemoveAt(flowStepItemIndex);
                    StateHasChanged();
                }
            }
        }

        private void RemoveConfiguredStep(int index)
        {
            _configuredFlowSteps.RemoveAt(index);
            StateHasChanged();
        }

        public List<FlowStepVM> GetConfiguredWorkflow()
        {
            return _configuredFlowSteps;
        }

        public List<Guid> GetConfiguredStepIds()
        {
            return _configuredFlowSteps
                .SelectMany(fs => fs.FlowStepItems)
                .Where(fsi => fsi.StepId.HasValue)
                .Select(fsi => fsi.StepId.Value)
                .ToList();
        }

        public void ClearConfiguration()
        {
            _configuredFlowSteps.Clear();
            _flowName = string.Empty;
            StateHasChanged();
        }


        public async Task SaveWorkflowInvokeAsync()
        {
            if (!IsWorkflowValid())
            {
                await _jsRuntime.InvokeVoidAsync("alert", "Please complete the workflow configuration.");
                return;
            }

            if (OnSaveWorkflow.HasDelegate)
            {
                await OnSaveWorkflow.InvokeAsync();
            }
        }

        public async Task<(Guid Id, string Name)?> SaveWorkflowFirst()
        {
            try
            {
                _isSaving = true;

                var apiResponse = await _flowService.PostFlowAsync(new PostFlowRequestDto
                {
                    FormTemplateId = null,
                    Name = _flowName,
                    FlowSteps = _configuredFlowSteps.Select(configuredStep => new PostFlowStepRequestDto
                    {
                        FlowStepItems = configuredStep.FlowStepItems.Select(fsi => new PostFlowStepItemRequestDto
                        {
                            StepId = fsi.StepId ?? Guid.Empty,
                            AssignedUsersIds = fsi.AssignedUsers?.Select(au => au.UserId ?? Guid.Empty).ToList() ?? new List<Guid>(),
                            AssignedTeams = fsi.AssignedTeams?.Select(at => new PostFlowTeamRequestDto
                            {
                                TeamId = at.TeamId ?? Guid.Empty,
                                UserIds = at.Team?.Users?.Select(au => au.Id).ToList() ?? new List<Guid>(),
                            }).ToList() ?? new List<PostFlowTeamRequestDto>(),
                        }).ToList(),
                    }).ToList(),
                });

                _isSaving = false;

                if (apiResponse != null && apiResponse.Success && apiResponse.Result != null)
                {
                    return (apiResponse.Result.Id, apiResponse.Result.Name ?? "Unnamed Flow");
                }

                return null;
            }
            catch (Exception ex)
            {
                _isSaving = false;
                throw;
            }
        }

        public void MoveStepUp(int index)
        {
            if (index > 0 && index < _configuredFlowSteps.Count)
            {
                var step = _configuredFlowSteps[index];
                _configuredFlowSteps.RemoveAt(index);
                _configuredFlowSteps.Insert(index - 1, step);
                StateHasChanged();
            }
        }

        public void MoveStepDown(int index)
        {
            if (index >= 0 && index < _configuredFlowSteps.Count - 1)
            {
                var step = _configuredFlowSteps[index];
                _configuredFlowSteps.RemoveAt(index);
                _configuredFlowSteps.Insert(index + 1, step);
                StateHasChanged();
            }
        }

        public int GetTotalUsersInWorkflow()
        {
            return _configuredFlowSteps.Sum(step =>
                step.FlowStepItems.Sum(flowStepItem =>
                    (flowStepItem.AssignedUsers?.Count ?? 0) +
                    (flowStepItem.AssignedTeams?.Count ?? 0)
                ));
        }

        public bool IsWorkflowValid()
        {
            return !string.IsNullOrWhiteSpace(_flowName) &&
                   _configuredFlowSteps.Any() &&
                   _configuredFlowSteps.All(fs => fs.FlowStepItems.Any() &&
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
            _flowStepItemToAssign = flowStepItem;
            _flowStepToAssignIndex = flowStepIndex;
            _flowStepItemToAssignIndex = flowStepItemIndex;
            StateHasChanged();
        }

        private void ConfigureStepsToFlow()
        {
            if (_flowStepToAssignIndex >= 0 && _flowStepToAssignIndex < _configuredFlowSteps.Count &&
                _flowStepItemToAssignIndex >= 0 && _flowStepItemToAssignIndex < _configuredFlowSteps[_flowStepToAssignIndex].FlowStepItems.Count)
            {
                FlowStepItemVM flowStepItem = _configuredFlowSteps[_flowStepToAssignIndex].FlowStepItems[_flowStepItemToAssignIndex];

                if (_flowStepItemToAssign != null)
                {
                    flowStepItem.AssignedUsers = _flowStepItemToAssign.AssignedUsers ?? new List<FlowStepItemUserVM>();
                    flowStepItem.AssignedTeams = _flowStepItemToAssign.AssignedTeams ?? new List<FlowStepItemTeamVM>();
                    flowStepItem.Step = _flowStepItemToAssign.Step;
                    flowStepItem.StepId = _flowStepItemToAssign.StepId;
                }

                StateHasChanged();
            }
        }

        public string GetFlowName()
        {
            return _flowName;
        }

        public async Task SetFlowSubmitMessageAsync(string message, bool success)
        {
            _onSubmitMessage = message;
            _onSubmitSuccess = success;

            StateHasChanged();

            await Task.Delay(3000);
            _onSubmitMessage = string.Empty;

            StateHasChanged();
        }
    }
}