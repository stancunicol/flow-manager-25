using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Application.DTOs.Requests.FormResponse
{
    public class PostFormResponseRequestDto
    {
        [Required(ErrorMessage = "FormTemplateId is required")]
        public Guid FormTemplateId { get; set; }

        [Required(ErrorMessage = "StepId is required")]
        public Guid StepId { get; set; }

        [Required(ErrorMessage = "UserId is required")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "ResponseFields is required")]
        public Dictionary<Guid, object> ResponseFields { get; set; } = new();
    }
}
