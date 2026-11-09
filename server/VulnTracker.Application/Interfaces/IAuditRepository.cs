using VulnTracker.Domain.Entities;

namespace VulnTracker.Application.Interfaces;

public interface IAuditRepository
{
    Task AddAsync(AuditEntry entry, CancellationToken ct);
    Task<IReadOnlyList<AuditEntry>> ListAsync(Guid? findingId, int take, CancellationToken ct);
}
