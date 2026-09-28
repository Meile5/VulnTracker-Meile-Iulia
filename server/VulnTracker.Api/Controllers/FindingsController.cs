using Microsoft.AspNetCore.Mvc;
using VulnTracker.Application.DTOs;
using VulnTracker.Application.Interfaces;
using VulnTracker.Application.Services;

namespace VulnTracker.Api.Controllers;

[ApiController]
[Route("api/findings")]
public class FindingsController(FindingService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<FindingResponse>>> Search(
        [FromQuery] FindingQuery query, CancellationToken ct) =>
        Ok(await service.SearchAsync(query, ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<FindingResponse>> Get(Guid id, CancellationToken ct)
    {
        var finding = await service.GetAsync(id, ct);
        return finding is null ? NotFound() : Ok(finding);
    }

    [HttpPost]
    public async Task<ActionResult<FindingResponse>> Create(
        CreateFindingRequest request, CancellationToken ct)
    {
        var created = await service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }
}