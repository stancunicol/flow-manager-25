using Microsoft.AspNetCore.Components;

namespace FlowManager.Client.Components.Admin.Flows
{
    public partial class Flows : ComponentBase
    {
        private string _activeTab = "VIEW";

        private void SetActiveTab(string tab)
        {
            _activeTab = tab;
        }   
    }
}
