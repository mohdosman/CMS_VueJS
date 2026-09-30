using CMS.Data.Constants;
using CMS.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Data.Configurations.Domain;

public class RecommendedTransportModeConfiguration : AuditableEntityConfiguration<RecommendedTransportMode>
{
    public override void Configure(EntityTypeBuilder<RecommendedTransportMode> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);
        entity.HasKey(e => e.RecommendedTransportModeId);

        entity.ToTable("CMS_RecommendedTransportMode", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.Property(e => e.RecommendedTransportModeId).ValueGeneratedNever();
        entity.Property(e => e.Abbreviation)
            .HasMaxLength(10)
            .IsUnicode(false);
        entity.Property(e => e.RecommendedTransportModeDescription)
            .HasColumnName("RecommendedTransportMode")
            .HasMaxLength(50)
            .IsUnicode(false);

    }
}

