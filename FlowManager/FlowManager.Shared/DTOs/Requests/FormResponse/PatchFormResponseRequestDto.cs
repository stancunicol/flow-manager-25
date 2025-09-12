using System.ComponentModel.DataAnnotations;

namespace FlowManager.Shared.DTOs.Requests.FormResponse
{
    public class PatchFormResponseRequestDto
    {
        [Required(ErrorMessage = "Id is required")]
        public Guid Id { get; set; }

        public Dictionary<Guid, object>? ResponseFields { get; set; }

        public Guid? ReviewerId { get; set; }

        [MaxLength(100, ErrorMessage = "RejectReason cannot exceed 100 characters")]
        public string? RejectReason { get; set; }
        public string? Status { get; set; }

        public Guid? StepId { get; set; }
    }
}
