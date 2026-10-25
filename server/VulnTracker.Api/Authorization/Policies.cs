namespace VulnTracker.Api.Authorization;

public static class Roles
{
    // must match the Keycloak realm role names exactly
    public const string Analyst = "analyst";
    public const string Developer = "developer";
    public const string PM = "pm";
    public const string Auditor = "auditor";
}

public static class Policies
{
    public const string CanViewFindings = nameof(CanViewFindings);
    public const string CanCreateFinding = nameof(CanCreateFinding);
    public const string CanUpdateFinding = nameof(CanUpdateFinding);
    public const string CanAssignFinding = nameof(CanAssignFinding);
    public const string CanSetSla = nameof(CanSetSla);
    public const string CanManageUsers = nameof(CanManageUsers);
    public const string CanViewAuditLogs = nameof(CanViewAuditLogs);
}
