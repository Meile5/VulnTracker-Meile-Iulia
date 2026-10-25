using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VulnTracker.Api.Contracts;

namespace VulnTracker.Api.Controllers;

[ApiController]
[Route("api/public/findings")]
[AllowAnonymous] // external reporter has no login, can only submit
public class PublicFindingsController : ControllerBase
{
    [HttpPost]
    public IActionResult Submit(PublicDisclosureRequest request) =>
        Accepted(); // nothing returned, so reporters can't read anything back
}
