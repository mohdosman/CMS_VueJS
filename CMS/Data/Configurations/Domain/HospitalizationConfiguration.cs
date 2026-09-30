using CMS.Data.Constants;
using CMS.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Data.Configurations.Domain;

public class HospitalizationConfiguration : AuditableEntityConfiguration<Hospitalization>
{
    public override void Configure(EntityTypeBuilder<Hospitalization> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);
        entity.HasKey(e => e.HospitalizationId) ;

        entity.ToTable("CMS_Hospitalization", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.Property(e => e.Abbreviation)
            .HasMaxLength(12)
            .IsUnicode(false)
            .IsFixedLength();

        entity.Property(e => e.HospitalizationDescription)
            .HasColumnName("Hospitalization")
            .HasMaxLength(150)
            .IsUnicode(false);

        entity.Property(e => e.NPI)
            .HasMaxLength(50)
            .IsUnicode(false);

    }
}

