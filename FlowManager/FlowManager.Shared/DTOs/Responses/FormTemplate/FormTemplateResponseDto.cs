using FlowManager.Shared.DTOs.Responses.FormTemplateComponent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Shared.DTOs.Responses.FormTemplate
{
    public class FormTemplateResponseDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Content { get; set; }
        public List<FormTemplateComponentResponseDto>? Components { get; set; } = new List<FormTemplateComponentResponseDto>();

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
