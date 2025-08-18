using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Shared.DTOs.Requests.FormTemplate
{
    public class PostFormTemplateRequestDto
    {
        public string Name { get; set; }
        public string Content { get; set; }
        public List<Guid> Components { get; set; } = new List<Guid>();
    }
}
