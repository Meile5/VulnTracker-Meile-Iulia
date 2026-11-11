using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using VulnTracker.Domain.Entities;
using VulnTracker.Infrastructure.Security;

namespace VulnTracker.Infrastructure.Database;

public class AppDbContext(DbContextOptions<AppDbContext> options, FieldEncryptor crypto) : DbContext(options)
{
    public DbSet<Finding> Findings => Set<Finding>();

    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();
    public DbSet<SlaPolicy> SlaPolicies => Set<SlaPolicy>();
    public DbSet<AuditLogEntry> AuditLog => Set<AuditLogEntry>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // AES-GCM on the sensitive text fields, title and asset stay readable for the list filters
        var description = new ValueConverter<string, string>(
            v => crypto.Encrypt(v, "Finding.Description"),
            v => crypto.Decrypt(v, "Finding.Description"));

        // ReporterEmail is nullable, so null stays null
        var reporterEmail = new ValueConverter<string?, string?>(
            v => v == null ? null : crypto.Encrypt(v, "Finding.ReporterEmail"),
            v => v == null ? null : crypto.Decrypt(v, "Finding.ReporterEmail"));

        b.Entity<Finding>().Property(f => f.Description).HasConversion(description);
        b.Entity<Finding>().Property(f => f.ReporterEmail).HasConversion(reporterEmail);
    }
}