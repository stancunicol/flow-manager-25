
namespace FlowManager.Shared.DTOs.Requests.StepHistory
{
    public class CreateStepHistoryRequestDto
    {
        public Guid StepId { get; set; }
        public string? NewName { get; set; }
        public string? OldDepartmentName { get; set; }
        public string? NewDepartmentName { get; set; }
        public List<string>? Users { get; set; }
        public string? FromDepartment { get; set; }
        public string? ToDepartment { get; set; }
    }
}