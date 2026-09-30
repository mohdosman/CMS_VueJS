using CMS.Data.Constants;
using CMS.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Data.Configurations.Domain;

public class RaceConfiguration : AuditableEntityConfiguration<Race>
{
    public override void Configure(EntityTypeBuilder<Race> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);
        entity.HasKey(e => e.RaceId);

        entity.ToTable("CMS_Race", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.Property(e => e.RaceId).ValueGeneratedNever();
        entity.Property(e => e.NOMSId)
            .HasMaxLength(2)
            .IsUnicode(false)
            .IsFixedLength();
        entity.Property(e => e.RaceDescription)
            .HasColumnName("Race")
            .HasMaxLength(120)
            .IsUnicode(false);

    }
}

