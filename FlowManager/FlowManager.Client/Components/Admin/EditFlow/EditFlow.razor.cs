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
        private List<StepVM> _configuredSteps = new List<StepVM>();
        private StepVM? _draggedStep = null;
        private bool _isDragOver = false;
        private string _flowName = string.Empty;
        private string _initialFlowName = string.Empty;

        private bool _showAssignToStepModal = false;
        private StepVM? _stepToAssign = null;

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
            _configuredSteps = response.Result.FlowSteps?.Select(fs => new StepVM
            {
                Id = fs.StepId ?? Guid.Empty,
                Name = fs.StepName ?? string.Empty,
                FlowStepId = fs.Id,
                Users = fs.Users?.Where(u => u.User?.Teams?.Count == 0 || u.User?.Teams == null)
                                .Select(u => new UserVM
                                {
                                    Id = u.User?.Id ?? Guid.Empty,
                                    Name = u.User?.Name ?? string.Empty,
                                    Email = u.User?.Email ?? string.Empty,
                                }).ToList() ?? new List<UserVM>(),
                Teams = fs.Teams?.Select(t => new TeamVM
                {
                    Id = t.Team?.Id ?? Guid.Empty,
                    Name = t.Team?.Name ?? string.Empty,
                    Users = t.Team?.Users?.Select(u => new UserVM
                    {
                        Id = u.Id,
                        Name = u.Name ?? string.Empty,
                        Email = u.Email ?? string.Empty,
                    }).ToList() ?? new List<UserVM>()
                }).ToList() ?? new List<TeamVM>(),
            }).ToList() ?? new List<StepVM>();
        }

        private async Task LoadStepsAsync()
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
                if (!_configuredSteps.Any(cs => cs.Id == _draggedStep.Id))
                {
                    var stepForWorkflow = new StepVM
                    {
                        Id = _draggedStep.Id,
                        Name = _draggedStep.Name,
                        Users = new List<UserVM>(),
                        Teams = new List<TeamVM>()
                    };
                    _configuredSteps.Add(stepForWorkflow);
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

        private async Task SaveCurrentWorkflowAsync()
        {
            PostFlowRequestDto payload = new PostFlowRequestDto
            {
                FormTemplateId = FormTemplateId,
                Name = _flowName,
                Steps = _configuredSteps.Select(configuredStep => new PostFlowStepRequestDto
                {
                    StepId = configuredStep.Id,
                    UserIds = configuredStep.Users!.Select(u => u.Id).ToList(),
                    Teams = configuredStep.Teams!.Select(t => new PostFlowTeamRequestDto
                    {
                        TeamId = t.Id,
                        UserIds = t.Users.Select(u => u.Id).ToList(),
                    }).ToList(),
                }).ToList()
            };

            ApiResponse<FlowResponseDto> response = await _flowService.PostFlowAsync(payload);

            _onSubmitMessage = response.Message;
            _onSubmitSuccess = response.Success;

            StateHasChanged();

            if(response.Success)
            {
                await Task.Delay(4000);
                _onSubmitMessage = string.Empty;
                await OnFlowSaved.InvokeAsync(null);
            }

            ClearConfiguration();
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
                (step.Users?.Count ?? 0) +
                (step.Teams?.Count ?? 0));
        }

        public bool IsWorkflowValid()
        {
            return !string.IsNullOrWhiteSpace(_flowName) &&
                   _flowName.ToUpper() != _initialFlowName.ToUpper() && 
                   _configuredSteps.Any() &&
                   _configuredSteps.All(s => !string.IsNullOrEmpty(s.Name) &&
                       ((s.Users != null && s.Users.Count > 0) || (s.Teams != null && s.Teams.Count > 0)));
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
            StateHasChanged();
        }

        private void ConfigureStepsToFlow()
        {
            StepVM step = _configuredSteps.First(s => s.Id == _stepToAssign!.Id);

            step.Users = _stepToAssign!.Users?.Select(u => new UserVM
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email
            }).ToList() ?? new List<UserVM>();

            step.Teams = _stepToAssign!.Teams?.Select(t => new TeamVM
            {
                Id = t.Id,
                Name = t.Name,
                Users = t.Users?.Select(u => new UserVM
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email
                }).ToList() ?? new List<UserVM>()
            }).ToList() ?? new List<TeamVM>();

            StateHasChanged();
        }
    }
}