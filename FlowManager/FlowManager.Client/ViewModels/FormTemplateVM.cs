using FlowManager.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace FlowManager.Client.ViewModels
{
    public class FormTemplateVM
    {
        public Guid Id { get; set; }

        public string? Name { get; set; }
        public string? Content { get; set; }

        public List<FormTemplateComponentVM>? Components { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
