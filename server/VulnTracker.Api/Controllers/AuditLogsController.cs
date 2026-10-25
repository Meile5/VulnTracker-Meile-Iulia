using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VulnTracker.Api.Authorization;

namespace VulnTracker.Api.Controllers;

[ApiController]
[Route("api/audit-logs")]
[Authorize(Policy = Policies.CanViewAuditLogs)]
public class AuditLogsController : ControllerBase
{
    [HttpGet]
    public IActionResult List() => Ok(Array.Empty<object>());
}
