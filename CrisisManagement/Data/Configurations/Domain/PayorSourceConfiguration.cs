using CrisisManagement.Data.Constants;
using CrisisManagement.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrisisManagement.Data.Configurations.Domain;

public class PayorSourceConfiguration : AuditableEntityConfiguration<PayorSource>
{
    public override void Configure(EntityTypeBuilder<PayorSource> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);
        entity.HasKey(e => e.PayorSourceId);

        entity.ToTable("CMS_PayorSource", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.Property(e => e.Abbreviation)
            .HasMaxLength(12)
            .IsUnicode(false)
            .IsFixedLength();
        entity.Property(e => e.PayorSourceDescription)
            .HasColumnName("PayorSource")
            .HasMaxLength(50)
            .IsUnicode(false);

    }
}

