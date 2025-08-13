using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Application.DTOs.Requests.FormResponse
{
    public class PatchFormResponseRequestDto
    {
        [Required(ErrorMessage = "Id is required")]
        public Guid Id { get; set; }

        public Dictionary<Guid, object>? ResponseFields { get; set; }

        [MaxLength(100, ErrorMessage = "RejectReason cannot exceed 100 characters")]
        public string? RejectReason { get; set; }

        public Guid? StepId { get; set; }
    }
}
