using FlowManager.Shared.DTOs.Responses.FormTemplate;
using FlowManager.Shared.DTOs.Requests.FormResponse;
using Microsoft.AspNetCore.Components;

namespace FlowManager.Client.Components.BasicUser
{
    public partial class FormCard : ComponentBase
    {
        [Parameter, EditorRequired] public FormResponseResponseDto FormResponse { get; set; } = default!;
        [Parameter] public EventCallback<FormResponseResponseDto> OnClick { get; set; }

        private string GetFormStatus()
        {
            if (!string.IsNullOrEmpty(FormResponse.Status))
            {
                return FormResponse.Status;
            }

            if (!string.IsNullOrEmpty(FormResponse.RejectReason))
            {
                return "Rejected";
            }

            return "Pending";
        }
    }
}
