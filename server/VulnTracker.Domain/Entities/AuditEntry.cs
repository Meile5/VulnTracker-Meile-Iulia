namespace VulnTracker.Domain.Entities;

public class AuditEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    public string Actor { get; set; } = "";   // who
    public string Action { get; set; } = "";  // did what, e.g. finding.updated
    public Guid? FindingId { get; set; }      // to which finding
    public string Details { get; set; } = ""; // what changed
}
