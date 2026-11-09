using VulnTracker.Domain.Entities;

namespace VulnTracker.Application.DTOs;

public record AuditEntryResponse(Guid Id, DateTimeOffset Timestamp, string Actor, string Action, Guid? FindingId, string Details)
{
    public static AuditEntryResponse From(AuditEntry e) =>
        new(e.Id, e.Timestamp, e.Actor, e.Action, e.FindingId, e.Details);
}

public record SlaResponse(string Severity, int Days);
