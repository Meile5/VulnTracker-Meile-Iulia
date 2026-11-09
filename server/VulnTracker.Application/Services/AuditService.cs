using VulnTracker.Application.DTOs;
using VulnTracker.Application.Interfaces;

namespace VulnTracker.Application.Services;

public class AuditService(IAuditRepository repo)
{
    public async Task<IReadOnlyList<AuditEntryResponse>> ListAsync(Guid? findingId, CancellationToken ct) =>
        (await repo.ListAsync(findingId, 500, ct)).Select(AuditEntryResponse.From).ToList();
}
