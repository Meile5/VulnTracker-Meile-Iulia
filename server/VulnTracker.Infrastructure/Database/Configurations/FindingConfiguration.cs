using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VulnTracker.Domain.Entities;

namespace VulnTracker.Infrastructure.Database.Configurations;

public class FindingConfiguration : IEntityTypeConfiguration<Finding>
{
    public void Configure(EntityTypeBuilder<Finding> b)
    {
        b.ToTable("findings");
        b.HasKey(f => f.Id);

        b.Property(f => f.Title).HasMaxLength(200).IsRequired();
        b.Property(f => f.Description).IsRequired();  
        b.Property(f => f.AffectedAsset).HasMaxLength(200).IsRequired();
        b.Property(f => f.CvssScore).HasPrecision(3, 1);
        b.Property(f => f.ReporterEmail).HasMaxLength(320);
        b.Property(f => f.AssigneeId).HasMaxLength(100);

        b.Property(f => f.Severity).HasConversion<string>().HasMaxLength(20);
        b.Property(f => f.Status).HasConversion<string>().HasMaxLength(20);
        b.Property(f => f.Source).HasConversion<string>().HasMaxLength(20);

        b.HasIndex(f => f.Status);
        b.HasIndex(f => f.Severity);
        b.HasIndex(f => f.AssigneeId);
        b.HasIndex(f => f.AffectedAsset);
    }
}