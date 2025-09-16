using FlowManager.Shared.DTOs.Responses.FlowStep;
using FlowManager.Shared.DTOs.Responses.FormTemplate;
using FlowManager.Shared.DTOs.Responses.Step;
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
        public string? Name { get; set; }
        public Guid? FormTemplateId { get; set; }
        public List<FlowStepResponseDto>? FlowSteps { get; set; }    
        public List<FormTemplateResponseDto>? FormTemplates { get; set; }
        public FormTemplateResponseDto? ActiveFormTemplate { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
