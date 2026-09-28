namespace VulnTracker.Domain.Entities;

public class AuditLogEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid FindingId { get; set; }
    public string ActorId { get; set; } = "";
    public string Action { get; set; } = "";
    public string? Details { get; set; }          // JSON
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}