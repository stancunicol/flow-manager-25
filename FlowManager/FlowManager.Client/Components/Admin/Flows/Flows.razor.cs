using Microsoft.AspNetCore.Components;

namespace FlowManager.Client.Components.Admin.Flows
{
    public partial class Flows : ComponentBase
    {
        
        [Parameter] public Guid? SavedFormTemplateId { get; set; }
        [Parameter] public string SavedFormTemplateName { get; set; } = "";

        
        private string _activeTab = "VIEW";

        private void SetActiveTab(string tab)
        {
            _activeTab = tab;
        }   
    }
}
