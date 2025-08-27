using FlowManager.Client.DTOs;
using FlowManager.Client.Services;
using FlowManager.Client.ViewModels;
using FlowManager.Client.ViewModels.Team;
using FlowManager.Shared.DTOs.Requests.Flow;
using FlowManager.Shared.DTOs.Requests.FlowStep;
using FlowManager.Shared.DTOs.Responses;
using FlowManager.Shared.DTOs.Responses.Flow;
using FlowManager.Shared.DTOs.Responses.Step;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;


namespace FlowManager.Client.Components.Admin.Flows.AddFlow.FlowAddModal
{
    public partial class FlowsAddModal : ComponentBase
    {
        [Inject] private StepService _stepService { get; set; } = default!;
        [Inject] private FlowService _flowService { get; set; } = default!;

        private List<StepVM> _availableSteps = new List<StepVM>();
        private List<StepVM> _configuredSteps = new List<StepVM>();
        private StepVM? _draggedStep = null;
        private bool _isDragOver = false;
        private string _flowName = string.Empty;

        private bool _showAssignToStepModal = false;
        private StepVM? _stepToAssign = null;

        private string _onSubmitMessage = string.Empty;
        private bool _onSubmitSuccess;

        protected override async Task OnInitializedAsync()
        {
            ApiResponse<PagedResponseDto<StepResponseDto>> response = await _stepService.GetStepsQueriedAsync();

            if (!response.Success)
            {
                return;
            }

            _availableSteps = response.Result.Data
                .Select(step => new StepVM
                {
                    Id = step.Id,
                    Name = step.Name,
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
                        Users = t.Users?.Select(u => new UserVM
                        {
                            Id = u.Id,
                            Name = u.Name,
                            Email = u.Email,
                        }).ToList() ?? new List<UserVM>(),
                    }).ToList() ?? new List<TeamVM>(),
                })
                .ToList();
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
                if (!_configuredSteps.Any(cs => cs.Id == _draggedStep.Id))
                {
                    _configuredSteps.Add(_draggedStep);
                }
            }

            _draggedStep = null;
            StateHasChanged();
        }

        private void RemoveConfiguredStep(Guid stepId)
        {
            _configuredSteps.RemoveAll(cs => cs.Id == stepId);
            StateHasChanged();
        }

        public List<StepVM> GetConfiguredWorkflow()
        {
            return _configuredSteps.ToList();
        }

        public List<Guid> GetConfiguredStepIds()
        {
            return _configuredSteps.Select(s => s.Id).ToList();
        }

        public void ClearConfiguration()
        {
            _configuredSteps.Clear();
            _flowName = string.Empty;
            StateHasChanged();
        }

        private bool IsStepConfigured(Guid stepId)
        {
            return _configuredSteps.Any(s => s.Id == stepId);
        }

        public async Task SaveWorkflow()
        {
            ApiResponse<FlowResponseDto> response = await _flowService.PostFlowAsync(new PostFlowRequestDto
            {
                Name = _flowName,
                Steps = _configuredSteps.Select(configuredStep => new PostFlowStepRequestDto
                {
                    StepId = configuredStep.Id,
                    UserIds = configuredStep.Users!.Select(u => u.Id).ToList(),
                    TeamIds = configuredStep.Teams!.Select(t => t.Id).ToList(),
                }).ToList()
            });

            _onSubmitMessage = response.Message;
            _onSubmitSuccess = response.Success;
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
                step.Users!.Count() + step.Teams!.Sum(t => t.Users!.Count()));
        }

        public bool IsWorkflowValid()
        {
            return !string.IsNullOrWhiteSpace(_flowName) && 
                   _configuredSteps.Any() &&
                   _configuredSteps.All(s => !string.IsNullOrEmpty(s.Name));
        }

        public string GetFlowNameValidationClass()
        {
            if (string.IsNullOrEmpty(_flowName))
                return "";

            return string.IsNullOrWhiteSpace(_flowName) ? "invalid" : "valid";
        }

        private void ShowAssingToStepModal(StepVM step)
        {
            _showAssignToStepModal = true;
            _stepToAssign = step;
        }

        private void ConfigureStepsToFlow()
        {
            StepVM step = _configuredSteps.First(s => s.Id == _stepToAssign!.Id);
            step.Users = _stepToAssign!.Users;
            step.Teams = _stepToAssign!.Teams;
        }
    }
}
