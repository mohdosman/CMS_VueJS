using CMS.Data.Constants;
using CMS.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Data.Configurations.Domain;

public class HospitalizationDispositionConfiguration : AuditableEntityConfiguration<HospitalizationDisposition>
{
    public override void Configure(EntityTypeBuilder<HospitalizationDisposition> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);
        entity.HasKey(e => e.HospitalizationDispositionId);

        entity.ToTable("CMS_HospitalizationDisposition", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.Property(e => e.Abbreviation)
            .HasMaxLength(12)
            .IsUnicode(false)
            .IsFixedLength();
        entity.Property(e => e.HospitalizationDispositionDescription)
            .HasColumnName("HospitalizationDisposition")
            .HasMaxLength(150)
            .IsUnicode(false);

    }
}

