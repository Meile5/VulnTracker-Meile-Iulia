using VulnTracker.Application.DTOs;
using VulnTracker.Application.Interfaces;
using VulnTracker.Domain.Entities;
using VulnTracker.Domain.Enums;

namespace VulnTracker.Application.Services;

public class FindingService(IFindingRepository repo)
{
    public async Task<FindingResponse> CreateAsync(CreateFindingRequest req, CancellationToken ct)
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
    
    // the controller needs the entity itself for the resource check
    public Task<Finding?> GetEntityAsync(Guid id, CancellationToken ct) => repo.GetAsync(id, ct);

public async Task<FindingResponse> UpdateAsync(
    Finding finding, string? title, string? description,
    FindingStatus? status, Severity? severity, CancellationToken ct)
{
    if (!string.IsNullOrWhiteSpace(title)) finding.Title = title.Trim();
    if (description is not null) finding.Description = description;
    if (status is not null) finding.Status = status.Value;
    if (severity is not null) finding.Severity = severity.Value; // triage: categorize severity
    finding.UpdatedAt = DateTimeOffset.UtcNow;

    await repo.SaveChangesAsync(ct);
    return FindingResponse.From(finding);
}

public async Task<FindingResponse> AssignAsync(
    Finding finding, string assigneeId, DateTimeOffset? dueDate, CancellationToken ct)
{
    finding.AssigneeId = assigneeId;
    finding.DueDate = dueDate; // the SLA target
    // a new finding that gets an owner counts as triaged
    if (finding.Status == FindingStatus.New) finding.Status = FindingStatus.Triaged;
    finding.UpdatedAt = DateTimeOffset.UtcNow;

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
    await repo.SaveChangesAsync(ct);
    return finding.Id;
}

public async Task<SummaryResponse> SummaryAsync(CancellationToken ct)
{
    // if this line doesn't compile, FindingQuery needs a different constructor, send me that file
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