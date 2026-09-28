namespace VulnTracker.Domain.Entities;

using VulnTracker.Domain.Enums;

public class Finding
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string AffectedAsset { get; set; } = "";
    public Severity Severity { get; set; }
    public decimal? CvssScore { get; set; }
    public DateTimeOffset DiscoveryDate { get; set; }
    public FindingStatus Status { get; set; } = FindingStatus.New;
    public FindingSource Source { get; set; }
    public string? ReporterEmail { get; set; }
    public string? AssigneeId { get; set; }      // Keycloak "sub" claim
    public DateTimeOffset? DueDate { get; set; } // SLA target
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}