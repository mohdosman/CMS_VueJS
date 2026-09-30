using CrisisManagement.Data.Constants;
using CrisisManagement.Shared.Constants;
using CrisisManagement.Data.Models.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrisisManagement.Data.Configurations.Identity;

public sealed class ReportConfiguration : AuditableEntityConfiguration<Report>
{
    public override void Configure(EntityTypeBuilder<Report> builder)
    {
        // Configure audit properties from base
        base.Configure(builder);

        // Table mapping
        builder.ToTable("RBS_Report", DatabaseConstants.DefaultRoleBaseSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        // Entity-specific properties
        builder.Property(e => e.ReportKey)
            .HasDefaultValueSql("(newid())");

        builder.Property(e => e.ReportName)
            .IsRequired()
            .HasMaxLength(ReportFieldLimits.MaxReportNameLength)
            .IsUnicode(false);

        builder.Property(e => e.FileName)
            .IsRequired()
            .HasMaxLength(ReportFieldLimits.MaxFileNameLength)
            .IsUnicode(false);

        builder.Property(e => e.Description)
            .IsRequired()
            .HasMaxLength(ReportFieldLimits.MaxDescriptionLength)
            .IsUnicode(false);

        builder.Property(e => e.ExportOption)
            .HasMaxLength(ReportFieldLimits.MaxExportOptionLength)
            .IsUnicode(false);
    }
}
