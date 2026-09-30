using CMS.Data.Constants;
using CMS.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Data.Configurations.Domain;

public class EthnicityConfiguration : AuditableEntityConfiguration<Ethnicity>
{
    public override void Configure(EntityTypeBuilder<Ethnicity> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);
        entity.HasKey(e => e.EthnicityId);

        entity.ToTable("CMS_Ethnicity", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.Property(e => e.EthnicityId).ValueGeneratedNever();
        entity.Property(e => e.EthnicityDescription)
            .HasColumnName("Ethnicity")
            .HasMaxLength(120)
            .IsUnicode(false);
        entity.Property(e => e.NOMSID)
            .HasMaxLength(2)
            .IsUnicode(false)
            .IsFixedLength();

    }
}

