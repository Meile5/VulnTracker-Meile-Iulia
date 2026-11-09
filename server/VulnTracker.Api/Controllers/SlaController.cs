using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VulnTracker.Api.Authorization;
using VulnTracker.Api.Contracts;
using VulnTracker.Application.DTOs;
using VulnTracker.Application.Services;
using VulnTracker.Domain.Enums;

namespace VulnTracker.Api.Controllers;

[ApiController]
[Route("api/sla")]
[Authorize]
public class SlaController(SlaService service) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = Policies.CanViewFindings)]
    public async Task<ActionResult<IReadOnlyList<SlaResponse>>> List(CancellationToken ct) =>
        Ok(await service.ListAsync(ct));

    [HttpPut]
    [Authorize(Policy = Policies.CanSetSla)]
    public async Task<IActionResult> Set(SetSlaRequest request, CancellationToken ct)
    {
        if (!Enum.TryParse<Severity>(request.Severity, true, out var severity) || request.Days is < 1 or > 365)
            return BadRequest("severity must be Low, Medium, High or Critical, days must be 1 to 365");

        await service.SetAsync(severity, request.Days, User.Identity?.Name ?? "unknown", ct);
        return Ok(await service.ListAsync(ct));
    }
}
