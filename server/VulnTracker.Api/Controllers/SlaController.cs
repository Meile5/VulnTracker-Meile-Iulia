using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VulnTracker.Api.Authorization;
using VulnTracker.Api.Contracts;

namespace VulnTracker.Api.Controllers;

[ApiController]
[Route("api/sla")]
[Authorize(Policy = Policies.CanSetSla)]
public class SlaController : ControllerBase
{
    [HttpPut]
    public IActionResult Set(SetSlaRequest request) => Ok();
}
