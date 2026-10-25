using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using VulnTracker.Api.Contracts;
using VulnTracker.Application.Services;

namespace VulnTracker.Api.Controllers;

[ApiController]
[Route("api/public/findings")]
[AllowAnonymous] // external reporter has no login, can only submit
[EnableRateLimiting("public-form")]
public class PublicFindingsController(FindingService service) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Submit(PublicDisclosureRequest request, CancellationToken ct)
    {
        var id = await service.SubmitExternalAsync(
            request.Title, request.Description, request.AffectedAsset, request.ContactEmail, ct);

        // acknowledgement: only a reference number, nothing readable about internal data
        return Accepted(new { reference = id });
    }
}
