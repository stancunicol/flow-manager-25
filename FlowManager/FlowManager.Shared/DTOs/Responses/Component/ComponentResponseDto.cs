
namespace FlowManager.Shared.DTOs.Responses.Component
{
    public class ComponentResponseDto
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string? Type { get; set; }
        public string? Label { get; set; }
        public bool? Required { get; set; }
        public Dictionary<string, object>? Properties { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
