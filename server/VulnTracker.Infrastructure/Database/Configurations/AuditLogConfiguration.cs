using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VulnTracker.Domain.Entities;

namespace VulnTracker.Infrastructure.Database.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLogEntry>
{
    public void Configure(EntityTypeBuilder<AuditLogEntry> b)
    {
        b.ToTable("audit_log");
        b.HasKey(a => a.Id);
        b.Property(a => a.ActorId).HasMaxLength(100).IsRequired();
        b.Property(a => a.Action).HasMaxLength(100).IsRequired();
        b.Property(a => a.Details).HasColumnType("jsonb");
        b.HasIndex(a => a.FindingId);
    }
}