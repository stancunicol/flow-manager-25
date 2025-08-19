using Microsoft.AspNetCore.Components;

namespace FlowManager.Client.Pages
{
    public partial class Admin: ComponentBase
    {
        private string _activeTab = "USERS";

        private void SetActiveTab(string tab)
        {
            _activeTab = tab;
        }

        private void Logout()
        {
        }
    }
}
