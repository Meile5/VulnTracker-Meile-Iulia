namespace VulnTracker.Application.Interfaces;

public record KeycloakUser(Guid Id, string Username, string? Email,
    string? FirstName, string? LastName, IReadOnlyList<string> Roles);

public interface IKeycloakAdminClient
{
    Task<IReadOnlyList<KeycloakUser>> ListUsersAsync(CancellationToken ct);
    Task<bool> UserExistsAsync(Guid id, CancellationToken ct);
    Task SetRolesAsync(Guid userId, IReadOnlyCollection<string> roles, CancellationToken ct);
}
