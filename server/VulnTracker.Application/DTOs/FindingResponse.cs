using VulnTracker.Domain.Entities;
using VulnTracker.Domain.Enums;

namespace VulnTracker.Application.DTOs;

public class FindingResponse
{
    public Guid Id { get; init; }
    public string Title { get; init; } = "";
    public string Description { get; init; } = "";
    public string AffectedAsset { get; init; } = "";
    public Severity Severity { get; init; }
    public decimal? CvssScore { get; init; }
    public DateTimeOffset DiscoveryDate { get; init; }
    public FindingStatus Status { get; init; }
    public FindingSource Source { get; init; }
    public string? AssigneeId { get; init; }
    public DateTimeOffset? DueDate { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }

    public static FindingResponse From(Finding f) => new()
    {
        Id = f.Id,
        Title = f.Title,
        Description = f.Description,
        AffectedAsset = f.AffectedAsset,
        Severity = f.Severity,
        CvssScore = f.CvssScore,
        DiscoveryDate = f.DiscoveryDate,
        Status = f.Status,
        Source = f.Source,
        AssigneeId = f.AssigneeId,
        DueDate = f.DueDate,
        CreatedAt = f.CreatedAt,
        UpdatedAt = f.UpdatedAt
    };
}