using Microsoft.EntityFrameworkCore;
using VulnTracker.Application.Interfaces;
using VulnTracker.Domain.Entities;
using VulnTracker.Domain.Enums;
using VulnTracker.Infrastructure.Database;

namespace VulnTracker.Infrastructure.Repositories;

public class SlaRepository(AppDbContext db) : ISlaRepository
{
    public async Task<IReadOnlyList<SlaPolicy>> ListAsync(CancellationToken ct) =>
        await db.SlaPolicies.AsNoTracking().ToListAsync(ct);

    public Task<SlaPolicy?> GetAsync(Severity severity, CancellationToken ct) =>
        db.SlaPolicies.FirstOrDefaultAsync(p => p.Severity == severity, ct);

    public async Task UpsertAsync(Severity severity, int days, CancellationToken ct)
    {
        var existing = await GetAsync(severity, ct);
        if (existing is null) await db.SlaPolicies.AddAsync(new SlaPolicy { Severity = severity, Days = days }, ct);
        else existing.Days = days;
    }

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
