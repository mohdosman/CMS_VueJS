using CrisisManagement.Data.Constants;
using CrisisManagement.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrisisManagement.Data.Configurations.Domain;

public class HospitalizationAlternativeConfiguration : AuditableEntityConfiguration<HospitalizationAlternative>
{
    public override void Configure(EntityTypeBuilder<HospitalizationAlternative> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);
        entity.HasKey(e => e.HospitalizationAlternativeId);

        entity.ToTable("CMS_HospitalizationAlternative", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.Property(e => e.Abbreviation)
            .HasMaxLength(12)
            .IsUnicode(false)
            .IsFixedLength();
        entity.Property(e => e.HospitalizationAlternativeDescription)
            .HasColumnName("HospitalizationAlternative")
            .HasMaxLength(50)
            .IsUnicode(false);

    }
}

