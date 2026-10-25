using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VulnTracker.Api.Authorization;
using VulnTracker.Api.Contracts;

namespace VulnTracker.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Policy = Policies.CanManageUsers)]
public class UsersController : ControllerBase
{
    // real version would call the Keycloak Admin API to change realm roles
    [HttpPut("{id}/roles")]
    public IActionResult UpdateRoles(string id, UpdateUserRolesRequest request) => Ok();
}
