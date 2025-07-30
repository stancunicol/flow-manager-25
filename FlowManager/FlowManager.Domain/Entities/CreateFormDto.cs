using System;

namespace FlowManager.Domain.Entities
{
    public class CreateFormDto
    {
        public Guid FlowId { get; set; }
        public Guid UserId { get; set; }
        public string Comment { get; set; } = string.Empty;
    }
}