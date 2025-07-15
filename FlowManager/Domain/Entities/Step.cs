namespace FlowManager.Domain.Entities
{
    public class Step
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<Guid> AssignedUserIds { get; set; } = new List<Guid>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
