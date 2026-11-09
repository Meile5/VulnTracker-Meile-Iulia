using Microsoft.EntityFrameworkCore;
using VulnTracker.Application.Interfaces;
using VulnTracker.Domain.Entities;
using VulnTracker.Infrastructure.Database;

namespace VulnTracker.Infrastructure.Repositories;

public class AuditRepository(AppDbContext db) : IAuditRepository
{
    
    public async Task AddAsync(AuditEntry entry, CancellationToken ct) =>
        await db.AuditEntries.AddAsync(entry, ct);

    public async Task<IReadOnlyList<AuditEntry>> ListAsync(Guid? findingId, int take, CancellationToken ct)
    {
        var query = db.AuditEntries.AsNoTracking().AsQueryable();
        if (findingId is not null) query = query.Where(e => e.FindingId == findingId);
        return await query.OrderByDescending(e => e.Timestamp).Take(take).ToListAsync(ct);
    }
}
