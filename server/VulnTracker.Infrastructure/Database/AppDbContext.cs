using Microsoft.EntityFrameworkCore;
using VulnTracker.Domain.Entities;

namespace VulnTracker.Infrastructure.Database;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Finding> Findings => Set<Finding>();
    
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();
    public DbSet<SlaPolicy> SlaPolicies => Set<SlaPolicy>();
    public DbSet<AuditLogEntry> AuditLog => Set<AuditLogEntry>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}