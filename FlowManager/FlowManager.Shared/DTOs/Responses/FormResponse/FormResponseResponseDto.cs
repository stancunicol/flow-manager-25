using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Shared.DTOs.Requests.FormResponse
{
    public class FormResponseResponseDto
    {
        public Guid Id { get; set; }
        public string? RejectReason { get; set; }
        public string? Status { get; set; } = "Pending";
        public Dictionary<Guid, object> ResponseFields { get; set; } = new();
        public Guid FormTemplateId { get; set; }
        public string? FormTemplateName { get; set; }
        public Guid StepId { get; set; }
        public string? StepName { get; set; }
        public Guid UserId { get; set; }
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
