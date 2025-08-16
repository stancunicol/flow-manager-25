using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Shared.DTOs.Responses.Step
{
    public class StepResponseDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public List<Guid>? UserIds { get; set; }
        public List<Guid>? FlowIds { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
