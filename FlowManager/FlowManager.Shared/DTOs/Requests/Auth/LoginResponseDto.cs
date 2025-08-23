using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Shared.DTOs.Requests.Auth
{
    public class LoginResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new List<string>();
    }
}
