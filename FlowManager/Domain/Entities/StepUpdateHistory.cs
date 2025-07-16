using System;

namespace FlowManager.Domain.Entities;

public class StepUpdateHistory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid StepId { get; set; }
    public Guid UserId { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string? Comment { get; set; }
    
    public virtual Step Step { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}