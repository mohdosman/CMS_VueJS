using CMS.Data.Constants;
using CMS.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Data.Configurations.Domain;

public class HospAltDispositionListConfiguration : AuditableEntityConfiguration<HospAltDispositionList>
{
    public override void Configure(EntityTypeBuilder<HospAltDispositionList> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);
        entity.HasKey(e => e.HospAltDispositionListId);

        entity.ToTable("CMS_HospAltDispositionList", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.Property(e => e.Abbreviation)
            .HasMaxLength(20)
            .IsUnicode(false);
        entity.Property(e => e.HospAltDisposition)
            .HasMaxLength(150)
            .IsUnicode(false);

    }
}

