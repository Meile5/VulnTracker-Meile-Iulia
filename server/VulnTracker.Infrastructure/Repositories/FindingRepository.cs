using Microsoft.EntityFrameworkCore;
using VulnTracker.Application.Interfaces;
using VulnTracker.Domain.Entities;
using VulnTracker.Infrastructure.Database;

namespace VulnTracker.Infrastructure.Repositories;

public class FindingRepository(AppDbContext db) : IFindingRepository
{
    public Task<Finding?> GetAsync(Guid id, CancellationToken ct) =>
        db.Findings.FirstOrDefaultAsync(f => f.Id == id, ct);

    public async Task<IReadOnlyList<Finding>> SearchAsync(FindingQuery q, CancellationToken ct)
    {
        var query = db.Findings.AsNoTracking().AsQueryable();

        if (q.Status is not null)   query = query.Where(f => f.Status == q.Status);
        if (q.Severity is not null) query = query.Where(f => f.Severity == q.Severity);
        if (q.AssigneeId is not null) query = query.Where(f => f.AssigneeId == q.AssigneeId);
        if (!string.IsNullOrWhiteSpace(q.Asset))
            query = query.Where(f => EF.Functions.ILike(f.AffectedAsset, $"%{q.Asset}%"));
        if (!string.IsNullOrWhiteSpace(q.Text))
            query = query.Where(f => EF.Functions.ILike(f.Title, $"%{q.Text}%"));

        return await query.OrderByDescending(f => f.CreatedAt).ToListAsync(ct);
    }

    public async Task AddAsync(Finding finding, CancellationToken ct) =>
        await db.Findings.AddAsync(finding, ct);

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}