
namespace FlowManager.Shared.DTOs.Requests.Component
{
    public class PatchComponentRequestDto
    {
        public string? Type { get; set; }
        public string? Label { get; set; }
        public bool? Required { get; set; }
        public Dictionary<string, object>? Properties { get; set; }
    }
}
