using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using VulnTracker.Domain.Entities;

namespace VulnTracker.Api.Authorization;

public static class FindingOperations
{
    public static readonly OperationAuthorizationRequirement Update = new() { Name = nameof(Update) };
}

public class FindingAuthorizationHandler
    : AuthorizationHandler<OperationAuthorizationRequirement, Finding>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext ctx, OperationAuthorizationRequirement req, Finding finding)
    {
        if (req.Name != FindingOperations.Update.Name) return Task.CompletedTask;

        var userId = ctx.User.FindFirstValue("sub") ?? ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);

        // analysts and PMs can update any finding
        if (ctx.User.IsInRole(Roles.Analyst) || ctx.User.IsInRole(Roles.PM))
            ctx.Succeed(req);
        // developers only the ones assigned to them
        else if (ctx.User.IsInRole(Roles.Developer) && userId is not null && finding.AssigneeId == userId)
            ctx.Succeed(req);

        // auditors never succeed here, so they stay read-only
        return Task.CompletedTask;
    }
}
