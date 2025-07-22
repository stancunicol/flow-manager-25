using Microsoft.AspNetCore.Components;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace FlowManager.Client.Pages
{
    public partial class Auth : ComponentBase
    {
        private string email = string.Empty;
        private string password = string.Empty;
        private string? errorMessage;

        [Inject]
        private HttpClient Http { get; set; }

        private async Task HandleLogin()
        {
            var body = new Dictionary<string, string>
                    {
                        { "email", email },
                        { "password", password }
                    };

            var response = await Http.PostAsJsonAsync("api/auth/login", body);

            if (response.IsSuccessStatusCode)
            {
                errorMessage = "You are logged in";
                
            }
            else
            {
                errorMessage = "Invalid email or password";
            }
        }

    }
}
