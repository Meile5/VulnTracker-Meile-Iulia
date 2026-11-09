using VulnTracker.Domain.Entities;
using VulnTracker.Domain.Enums;

namespace VulnTracker.Application.Interfaces;

public interface ISlaRepository
{
    Task<IReadOnlyList<SlaPolicy>> ListAsync(CancellationToken ct);
    Task<SlaPolicy?> GetAsync(Severity severity, CancellationToken ct);
    Task UpsertAsync(Severity severity, int days, CancellationToken ct); // does not save
    Task SaveChangesAsync(CancellationToken ct);
}
