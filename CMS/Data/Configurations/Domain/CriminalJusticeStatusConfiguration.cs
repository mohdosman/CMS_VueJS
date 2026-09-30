using CMS.Data.Constants;
using CMS.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Data.Configurations.Domain;

public class CriminalJusticeStatusConfiguration : AuditableEntityConfiguration<CriminalJusticeStatus>
{
    public override void Configure(EntityTypeBuilder<CriminalJusticeStatus> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);

        entity.ToTable("CMS_CriminalJusticeStatus", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.Property(e => e.CriminalJusticeStatusDescription)
            .HasColumnName("CriminalJusticeStatus")
            .HasMaxLength(150)
            .IsUnicode(false);
        entity.Property(e => e.CriminalJusticeStatusCode)
            .HasMaxLength(50)
            .IsUnicode(false);
        entity.Property(e => e.NOMSId)
            .HasMaxLength(2)
            .IsUnicode(false)
            .IsFixedLength();

    }
}

