using FlowManager.Client.DTOs;
using FlowManager.Client.Services;
using FlowManager.Client.ViewModels;
using FlowManager.Shared.DTOs.Requests.User;
using FlowManager.Shared.DTOs.Responses;
using FlowManager.Shared.DTOs.Responses.User;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace FlowManager.Client.Components.Admin.Members
{
    public partial class Members : ComponentBase
    {
        private string _activeTab = "VIEWUSERS";

        private void SetActiveTab(string tabName)
        {
            _activeTab = tabName;
        }   
    }
}
