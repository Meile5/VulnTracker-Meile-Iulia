using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VulnTracker.Api.Authorization;
using VulnTracker.Application.DTOs;
using VulnTracker.Application.Services;

namespace VulnTracker.Api.Controllers;

[ApiController]
[Route("api/audit-logs")]
[Authorize(Policy = Policies.CanViewAuditLogs)]
public class AuditLogsController(AuditService service) : ControllerBase
{
    // GET /api/audit-logs or /api/audit-logs?findingId=...
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AuditEntryResponse>>> List(
        [FromQuery] Guid? findingId, CancellationToken ct) =>
        Ok(await service.ListAsync(findingId, ct));
}
