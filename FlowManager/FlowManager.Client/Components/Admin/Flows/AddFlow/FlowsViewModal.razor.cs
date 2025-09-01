using FlowManager.Client.Services;
using FlowManager.Shared.DTOs.Requests;
using FlowManager.Shared.DTOs.Requests.Flow;
using FlowManager.Shared.DTOs.Responses.Flow;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace FlowManager.Client.Components.Admin.Flows.AddFlow
{
    public partial class FlowsViewModal : ComponentBase
    {
        [Parameter] public EventCallback<FlowResponseDto> OnEditFlowRequested { get; set; }
        [Inject] private FlowService _flowService { get; set; } = default!;

        private List<FlowResponseDto> _flows = new();
        private bool _isLoading = false;
        private string _searchTerm = string.Empty;
        private bool _showEditModal = false;
        private FlowResponseDto? _selectedFlow = null;


        protected override async Task OnInitializedAsync()
        {
            await LoadFlows();
        }

        private async Task LoadFlows()
        {
            _isLoading = true;
            StateHasChanged();

            try
            {
                var payload = new QueriedFlowRequestDto
                {
                    Name = string.IsNullOrWhiteSpace(_searchTerm) ? null : _searchTerm,
                    QueryParams = new QueryParamsDto
                    {
                        Page = 1,
                        PageSize = 50
                    }
                };

                var response = await _flowService.GetAllFlowsQueriedAsync(payload);

                if (response.Success && response.Result != null)
                {
                    _flows = response.Result.Data.ToList();
                }
                else
                {
                    _flows = new List<FlowResponseDto>();
                }

                Console.WriteLine($"Flows loaded: {_flows.Count} items.");
            }
            catch (Exception ex)
            {   
                _flows = new List<FlowResponseDto>();
            }
            finally
            {
                _isLoading = false;
                StateHasChanged();
            }
        }
        public async Task RefreshFlows()
        {
            await LoadFlows();
        }

        private async Task SearchFlows()
        {
            await LoadFlows();
        }

        private async Task OnEnterPressed(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
            {
                await SearchFlows();
            }
        }

        private int GetTotalUsers(FlowResponseDto flow)
        {
            if (flow.Steps?.Any() != true) return 0;

            return flow.Steps.Sum(step =>
                (step.Users?.Count() ?? 0) +
                (step.Teams?.Sum(t => t.Users?.Count() ?? 0) ?? 0)
            );
        }
        


        private void OpenEditModal(FlowResponseDto flow)
        {
            if (OnEditFlowRequested.HasDelegate)
            {
                OnEditFlowRequested.InvokeAsync(flow);
                StateHasChanged();
            }
            else
            {
                // Altfel funcționăm local (pentru Flows.razor)
                _selectedFlow = flow;
                _showEditModal = true;
                StateHasChanged();
            }
        }
        private async Task CloseEditModal()
        {
            _showEditModal = false;
            _selectedFlow = null;
            StateHasChanged();
        }

        private async Task OnFlowUpdated(FlowResponseDto updatedFlow)
        {
            var index = _flows.FindIndex(f => f.Id == updatedFlow.Id);
            if (index >= 0)
            {
                _flows[index] = updatedFlow;
            }

            await CloseEditModal();
            await LoadFlows();
        }


    }
}
