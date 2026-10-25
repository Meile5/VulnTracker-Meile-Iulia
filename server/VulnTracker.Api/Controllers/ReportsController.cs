using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VulnTracker.Api.Authorization;
using VulnTracker.Application.DTOs;
using VulnTracker.Application.Services;

namespace VulnTracker.Api.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize(Policy = Policies.CanViewFindings)]
public class ReportsController(FindingService service) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<ActionResult<SummaryResponse>> Summary(CancellationToken ct) =>
        Ok(await service.SummaryAsync(ct));
}
