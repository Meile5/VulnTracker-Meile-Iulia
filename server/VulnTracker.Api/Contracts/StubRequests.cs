using System.ComponentModel.DataAnnotations;
using VulnTracker.Domain.Enums;

namespace VulnTracker.Api.Contracts;

public record UpdateFindingRequest(string? Title, string? Description, FindingStatus? Status, Severity? Severity);
public record AssignFindingRequest(string AssigneeId, DateTimeOffset? DueDate);
public record SetSlaRequest(string Severity, int Days);
public record UpdateUserRolesRequest(string[] Roles);

// a class so the validation attributes work, this form is open to the internet
public class PublicDisclosureRequest
{
    [Required, StringLength(200)] public string Title { get; init; } = "";
    [Required, StringLength(5000)] public string Description { get; init; } = "";
    [Required, StringLength(200)] public string AffectedAsset { get; init; } = "";
    [Required, EmailAddress, StringLength(254)] public string ContactEmail { get; init; } = "";
}
