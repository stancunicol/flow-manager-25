using System.ComponentModel.DataAnnotations;

namespace FlowManager.Shared.DTOs.Requests.FormResponse
{
    public class PostFormResponseRequestDto
    {
        [Required(ErrorMessage = "FormTemplateId is required")]
        public Guid FormTemplateId { get; set; }

        [Required(ErrorMessage = "StepId is required")]
        public Guid StepId { get; set; }

        [Required(ErrorMessage = "UserId is required")]
        public Guid UserId { get; set; }
        public Guid? CompletedByOtherUserId { get; set; }

        [Required(ErrorMessage = "ResponseFields is required")]
        public Dictionary<Guid, object> ResponseFields { get; set; } = new();
    }
}
