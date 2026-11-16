using Microsoft.EntityFrameworkCore;
using VulnTracker.Domain.Entities;
using VulnTracker.Domain.Enums;
using VulnTracker.Infrastructure.Database;

namespace VulnTracker.Infrastructure.Seed;

public static class DemoDataSeeder
{
    // all data is fake, goes through EF so Description and ReporterEmail get encrypted
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Findings.AnyAsync()) return; // only seed an empty table

        var now = DateTimeOffset.UtcNow;

        Finding Make(string title, string desc, string asset, Severity sev, decimal? cvss,
                     FindingStatus status, FindingSource source, int daysAgo,
                     string? assignee = null, int? dueInDays = null, string? email = null) => new()
        {
            Title = title,
            Description = desc,
            AffectedAsset = asset,
            Severity = sev,
            CvssScore = cvss,
            Status = status,
            Source = source,
            DiscoveryDate = now.AddDays(-daysAgo),
            AssigneeId = assignee,
            DueDate = dueInDays is null ? null : now.AddDays(dueInDays.Value),
            ReporterEmail = email,
            CreatedAt = now.AddDays(-daysAgo),
            UpdatedAt = now.AddDays(-daysAgo)
        };

        db.Findings.AddRange(
            Make("SQL injection in login form", "Demo finding: user input reaches the query unfiltered.",
                 "demo-shop.example/login", Severity.Critical, 9.8m, FindingStatus.Triaged, FindingSource.Internal, 10,
                 "demo-assignee-1", -3),
            Make("Outdated TLS version", "Demo finding: server still accepts TLS 1.0.",
                 "demo-api.example", Severity.High, 7.4m, FindingStatus.Triaged, FindingSource.Internal, 8,
                 "demo-assignee-2", 5),
            Make("Missing security headers", "Demo finding: no CSP or X-Frame-Options.",
                 "demo-web.example", Severity.Medium, 5.3m, FindingStatus.New, FindingSource.Internal, 5),
            Make("Verbose error messages", "Demo finding: stack traces shown to users.",
                 "demo-api.example/errors", Severity.Low, 3.1m, FindingStatus.New, FindingSource.Internal, 4),
            Make("Weak password policy", "Demo finding: 4 character passwords accepted.",
                 "demo-portal.example", Severity.High, 7.1m, FindingStatus.Remediated, FindingSource.Internal, 20,
                 "demo-assignee-1", -10),
            Make("Open admin panel", "Demo finding: admin page reachable without login.",
                 "demo-admin.example", Severity.Critical, 9.1m, FindingStatus.AcceptedRisk, FindingSource.Internal, 15),
            Make("XSS in comment box", "Reported by a visitor: script runs when comment is shown.",
                 "demo-blog.example/comments", Severity.Medium, null, FindingStatus.New, FindingSource.External, 2,
                 email: "demo.reporter1@example.com"),
            Make("Exposed backup file", "Reported by a visitor: backup.zip downloadable from the web root.",
                 "demo-files.example", Severity.Medium, null, FindingStatus.New, FindingSource.External, 1,
                 email: "demo.reporter2@example.com")
        );

        await db.SaveChangesAsync();
    }
}
