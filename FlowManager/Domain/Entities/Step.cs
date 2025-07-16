namespace FlowManager.Domain.Entities
{
    public class Step
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public List<Guid> AssignedUserIds { get; set; } = new List<Guid>();
        public Dictionary<Guid, DateTime> UpdateHistory { get; set; } = new Dictionary<Guid, DateTime>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
    }
}
