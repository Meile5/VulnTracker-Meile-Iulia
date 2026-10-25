using VulnTracker.Domain.Enums;

namespace VulnTracker.Api.Contracts;

public record UpdateFindingRequest(string? Title, string? Description, FindingStatus? Status); // was string? Status
public record AssignFindingRequest(string AssigneeId); // Keycloak user ID (sub), was AssigneeUsername
public record SetSlaRequest(string Severity, int Days);
public record UpdateUserRolesRequest(string[] Roles);
public record PublicDisclosureRequest(string Title, string Description, string AffectedAsset, string? ContactEmail);
