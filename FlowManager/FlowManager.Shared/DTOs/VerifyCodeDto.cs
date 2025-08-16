using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Shared.DTOs
{
    public class VerifyCodeDto
    {
        public string Email { get; set; }
        public string Code { get; set; }
    }
}
