using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Shared.DTOs.Responses.Flow
{
    public class FlowResponseDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; } = string.Empty;
        public Guid? FormTemplateId { get; set; }
        public List<Guid>? StepIds { get; set; }  
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
