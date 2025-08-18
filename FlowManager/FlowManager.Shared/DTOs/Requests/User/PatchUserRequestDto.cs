using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Shared.DTOs.Requests.User
{
    public class PatchUserRequestDto
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public List<Guid>? Roles { get; set; }
        public Guid? TeamId { get; set; }
    }
}
