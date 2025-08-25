using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using FlowManager.Shared.DTOs.Responses.Step;
using FlowManager.Client.Services;
using FlowManager.Domain.Entities;

namespace FlowManager.Client.Components.Admin.Flows.AddFlow
{
    public partial class CreateStep : ComponentBase
    {
        private List<StepResponseDto> steps = new();
        private string newStepName = string.Empty;
        private bool showModal = false;

        [Inject]
        private StepService stepService { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            await LoadSteps();
        }

        private async Task LoadSteps()
        {
            try
            {
                var response = await stepService.GetStepsQueriedAsync();
                if (response != null && response.Success && response.Result != null)
                {
                    steps = (List<StepResponseDto>)response.Result.Data;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading steps: {ex.Message}");
            }
        }

        private void OpenModal()
        {
            showModal = true;
            Console.WriteLine(">>> OpenModal CALLED <<<");
            newStepName = string.Empty;
            StateHasChanged();
        }

        private void CloseModal()
        {
            showModal = false;
            newStepName = string.Empty;
            StateHasChanged();
        }

        private async Task SaveStep()
        {
            if (string.IsNullOrWhiteSpace(newStepName))
                return;

            try
            {
                var step = new Step
                {
                    Name = newStepName.Trim(),
                    FlowSteps = new List<FlowStep>(),
                    Users = new List<StepUser>(),
                    Teams = new List<StepTeam>()
                };

                var result = await stepService.CreateStepAsync(step);

                if (result != null)
                {
                    steps.Add(new StepResponseDto { Name = newStepName.Trim() });

                    newStepName = string.Empty;
                    showModal = false;
                    StateHasChanged();

                    Console.WriteLine("Step created successfully!");
                }
                else
                {
                    Console.WriteLine("Failed to create step");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving step: {ex.Message}");
            }
        }

        private async Task HandleKeyPress(KeyboardEventArgs e)
        {
            if (e.Key == "Enter" && !string.IsNullOrWhiteSpace(newStepName))
            {
                await SaveStep();
            }
            else if (e.Key == "Escape")
            {
                CloseModal();
            }
        }
    }
}