using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VulnTracker.Api.Authorization;
using VulnTracker.Api.Contracts;
using VulnTracker.Application.DTOs;
using VulnTracker.Application.Interfaces;
using VulnTracker.Application.Services;

namespace VulnTracker.Api.Controllers;

[ApiController]
[Route("api/findings")]
[Authorize]
public class FindingsController(FindingService service, IAuthorizationService authz) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = Policies.CanViewFindings)]
    public async Task<ActionResult<IReadOnlyList<FindingResponse>>> Search(
        [FromQuery] FindingQuery query, CancellationToken ct) =>
        Ok(await service.SearchAsync(query, ct));

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Policies.CanViewFindings)]
    public async Task<ActionResult<FindingResponse>> Get(Guid id, CancellationToken ct)
    {
        var finding = await service.GetAsync(id, ct);
        return finding is null ? NotFound() : Ok(finding);
    }

    [HttpPost]
    [Authorize(Policy = Policies.CanCreateFinding)]
    public async Task<ActionResult<FindingResponse>> Create(
        CreateFindingRequest request, CancellationToken ct)
    {
        var created = await service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }
    

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Policies.CanUpdateFinding)]
    public async Task<ActionResult<FindingResponse>> Update(
        Guid id, UpdateFindingRequest request, CancellationToken ct)
    {
        var finding = await service.GetEntityAsync(id, ct);
        if (finding is null) return NotFound();

        // resource check: developers only pass for findings assigned to them
        var result = await authz.AuthorizeAsync(User, finding, FindingOperations.Update);
        if (!result.Succeeded) return Forbid();

        return Ok(await service.UpdateAsync(finding, request.Title, request.Description, request.Status, request.Severity, ct));
    }

    [HttpPost("{id:guid}/assign")]
    [Authorize(Policy = Policies.CanAssignFinding)]
    public async Task<ActionResult<FindingResponse>> Assign(
        Guid id, AssignFindingRequest request, CancellationToken ct)
    {
        var finding = await service.GetEntityAsync(id, ct);
        if (finding is null) return NotFound();

        return Ok(await service.AssignAsync(finding, request.AssigneeId, request.DueDate, ct));
    }
}
