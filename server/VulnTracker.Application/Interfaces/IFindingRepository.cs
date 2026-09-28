using VulnTracker.Domain.Entities;
using VulnTracker.Domain.Enums;

namespace VulnTracker.Application.Interfaces;

public record FindingQuery(
    FindingStatus? Status = null,
    Severity? Severity = null,
    string? AssigneeId = null,
    string? Asset = null,
    string? Text = null);

public interface IFindingRepository
{
    Task<Finding?> GetAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<Finding>> SearchAsync(FindingQuery query, CancellationToken ct);
    Task AddAsync(Finding finding, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}