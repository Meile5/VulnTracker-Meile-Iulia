using VulnTracker.Application.DTOs;
using VulnTracker.Application.Interfaces;
using VulnTracker.Domain.Entities;
using VulnTracker.Domain.Enums;

namespace VulnTracker.Application.Services;

public class SlaService(ISlaRepository repo, IAuditRepository audit)
{
    private static readonly Dictionary<Severity, int> Defaults = new()
    {
        [Severity.Critical] = 7, [Severity.High] = 30, [Severity.Medium] = 90, [Severity.Low] = 180,
    };

    public async Task<IReadOnlyList<SlaResponse>> ListAsync(CancellationToken ct)
    {
        var stored = (await repo.ListAsync(ct)).ToDictionary(p => p.Severity, p => p.Days);
        return Enum.GetValues<Severity>()
            .Select(s => new SlaResponse(s.ToString(), stored.GetValueOrDefault(s, Defaults[s])))
            .ToList();
    }

    public async Task<int> DaysForAsync(Severity severity, CancellationToken ct) =>
        (await repo.GetAsync(severity, ct))?.Days ?? Defaults[severity];

    public async Task SetAsync(Severity severity, int days, string actor, CancellationToken ct)
    {
        var old = await DaysForAsync(severity, ct);
        await repo.UpsertAsync(severity, days, ct);
        await audit.AddAsync(new AuditEntry
        {
            Actor = actor, Action = "sla.updated", Details = $"{severity}: {old} -> {days} days",
        }, ct);
        await repo.SaveChangesAsync(ct); 
    }
}
