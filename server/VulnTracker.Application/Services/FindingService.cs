using VulnTracker.Application.DTOs;
using VulnTracker.Application.Interfaces;
using VulnTracker.Domain.Entities;
using VulnTracker.Domain.Enums;

namespace VulnTracker.Application.Services;

public class FindingService(IFindingRepository repo, IAuditRepository audit, SlaService sla)
{
    private Task LogAsync(string actor, string action, Guid findingId, string details, CancellationToken ct) =>
        audit.AddAsync(new AuditEntry { Actor = actor, Action = action, FindingId = findingId, Details = details }, ct);

    public async Task<FindingResponse> CreateAsync(CreateFindingRequest req, string actor, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        var finding = new Finding
        {
            Title = req.Title.Trim(),
            Description = req.Description,
            AffectedAsset = req.AffectedAsset.Trim(),
            Severity = req.Severity,
            CvssScore = req.CvssScore,
            DiscoveryDate = req.DiscoveryDate ?? now,
            Source = FindingSource.Internal,
            Status = FindingStatus.New,
            CreatedAt = now,
            UpdatedAt = now
        };

        await repo.AddAsync(finding, ct);
        await LogAsync(actor, "finding.created", finding.Id, $"severity {finding.Severity}, asset '{finding.AffectedAsset}'", ct);
        await repo.SaveChangesAsync(ct);
        return FindingResponse.From(finding);
    }

    public async Task<FindingResponse?> GetAsync(Guid id, CancellationToken ct)
    {
        var finding = await repo.GetAsync(id, ct);
        return finding is null ? null : FindingResponse.From(finding);
    }

    public async Task<IReadOnlyList<FindingResponse>> SearchAsync(FindingQuery query, CancellationToken ct)
    {
        var findings = await repo.SearchAsync(query, ct);
        return findings.Select(FindingResponse.From).ToList();
    }

    public Task<Finding?> GetEntityAsync(Guid id, CancellationToken ct) => repo.GetAsync(id, ct);

    public async Task<FindingResponse> UpdateAsync(
        Finding finding, string? title, string? description,
        FindingStatus? status, Severity? severity, string actor, CancellationToken ct)
    {
        var changes = new List<string>();

        if (!string.IsNullOrWhiteSpace(title) && title.Trim() != finding.Title)
        {
            changes.Add($"title '{finding.Title}' -> '{title.Trim()}'");
            finding.Title = title.Trim();
        }
        if (description is not null && description != finding.Description)
        {
            changes.Add("description edited"); 
            finding.Description = description;
        }
        if (status is not null && status != finding.Status)
        {
            changes.Add($"status {finding.Status} -> {status}");
            finding.Status = status.Value;
        }
        if (severity is not null && severity != finding.Severity)
        {
            changes.Add($"severity {finding.Severity} -> {severity}");
            finding.Severity = severity.Value;
        }

        if (changes.Count > 0)
        {
            finding.UpdatedAt = DateTimeOffset.UtcNow;
            await LogAsync(actor, "finding.updated", finding.Id, string.Join("; ", changes), ct);
            await repo.SaveChangesAsync(ct);
        }
        return FindingResponse.From(finding);
    }

    public async Task<FindingResponse> AssignAsync(
        Finding finding, string assigneeId, DateTimeOffset? dueDate, string actor, CancellationToken ct)
    {
        var oldAssignee = finding.AssigneeId ?? "nobody";
        var due = dueDate ?? DateTimeOffset.UtcNow.AddDays(await sla.DaysForAsync(finding.Severity, ct));

        finding.AssigneeId = assigneeId;
        finding.DueDate = due;
        var details = $"assignee {oldAssignee} -> {assigneeId}; due {due:yyyy-MM-dd}";
        // a new finding that gets an owner counts as triaged
        if (finding.Status == FindingStatus.New)
        {
            finding.Status = FindingStatus.Triaged;
            details += "; status New -> Triaged";
        }
        finding.UpdatedAt = DateTimeOffset.UtcNow;

        await LogAsync(actor, "finding.assigned", finding.Id, details, ct);
        await repo.SaveChangesAsync(ct);
        return FindingResponse.From(finding);
    }

    // public form: the reporter has no say in severity, the analyst sets it during triage
    public async Task<Guid> SubmitExternalAsync(
        string title, string description, string asset, string email, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        var finding = new Finding
        {
            Title = title.Trim(),
            Description = description,
            AffectedAsset = asset.Trim(),
            Severity = Severity.Medium, // placeholder until triage
            DiscoveryDate = now,
            Source = FindingSource.External,
            Status = FindingStatus.New,
            ReporterEmail = email.Trim(),
            CreatedAt = now,
            UpdatedAt = now
        };

        await repo.AddAsync(finding, ct);
        // the reporter's email stays out of the audit log
        await LogAsync("external-reporter", "finding.submitted", finding.Id, $"via public form, asset '{finding.AffectedAsset}'", ct);
        await repo.SaveChangesAsync(ct);
        return finding.Id;
    }

    public async Task<SummaryResponse> SummaryAsync(CancellationToken ct)
    {
        var all = await repo.SearchAsync(new FindingQuery(), ct);
        var now = DateTimeOffset.UtcNow;
        var open = all.Where(f => f.Status is not (FindingStatus.Remediated or FindingStatus.AcceptedRisk)).ToList();

        return new SummaryResponse
        {
            Open = open.Count,
            Overdue = open.Count(f => f.DueDate < now),
            OpenBySeverity = Enum.GetValues<Severity>().ToDictionary(s => s.ToString(), s => open.Count(f => f.Severity == s)),
            ByStatus = Enum.GetValues<FindingStatus>().ToDictionary(s => s.ToString(), s => all.Count(f => f.Status == s)),
        };
    }
}
