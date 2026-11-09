using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VulnTracker.Api.Authorization;
using VulnTracker.Application.Interfaces;

namespace VulnTracker.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController(IKeycloakAdminClient kc) : ControllerBase
{
    public record UserResponse(Guid Id, string Username, string? Email, string DisplayName, IReadOnlyList<string> Roles);
    public record SetRolesRequest(string[] Roles);

    // assignee picker (analyst + pm) and roles panel (pm)
    [HttpGet]
    [Authorize(Policy = Policies.CanAssignFinding)]
    public async Task<ActionResult<IReadOnlyList<UserResponse>>> List(CancellationToken ct) =>
        Ok((await kc.ListUsersAsync(ct)).Select(u => new UserResponse(
            u.Id, u.Username, u.Email,
            string.Join(' ', new[] { u.FirstName, u.LastName }.Where(s => !string.IsNullOrWhiteSpace(s))) is { Length: > 0 } n ? n : u.Username,
            u.Roles)));

    [HttpPut("{id:guid}/roles")]
    [Authorize(Policy = Policies.CanManageUsers)]
    public async Task<IActionResult> SetRoles(Guid id, SetRolesRequest req, CancellationToken ct)
    {
        // don't let a PM remove their own PM role and lock themselves out
        if (User.FindFirst("sub")?.Value == id.ToString() && !req.Roles.Contains(Roles.PM))
            return BadRequest(new { error = "You cannot remove your own PM role." });

        if (!await kc.UserExistsAsync(id, ct)) return NotFound();

        await kc.SetRolesAsync(id, req.Roles, ct);
        return NoContent();
    }
}
