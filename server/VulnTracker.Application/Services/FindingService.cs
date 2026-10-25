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
        Finding finding, string? title, string? description, FindingStatus? status, CancellationToken ct)
    {
        // only overwrite what was sent, so partial updates work
        if (!string.IsNullOrWhiteSpace(title)) finding.Title = title.Trim();
        if (description is not null) finding.Description = description;
        if (status is not null) finding.Status = status.Value;
        finding.UpdatedAt = DateTimeOffset.UtcNow;

        await repo.SaveChangesAsync(ct);
        return FindingResponse.From(finding);
    }

    public async Task<FindingResponse> AssignAsync(Finding finding, string assigneeId, CancellationToken ct)
    {
        finding.AssigneeId = assigneeId;
        finding.UpdatedAt = DateTimeOffset.UtcNow;

        await repo.SaveChangesAsync(ct);
        return FindingResponse.From(finding);
    }
    
}