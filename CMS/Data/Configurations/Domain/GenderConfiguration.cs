using CMS.Data.Constants;
using CMS.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Data.Configurations.Domain;

public class GenderConfiguration : AuditableEntityConfiguration<Gender>
{
    public override void Configure(EntityTypeBuilder<Gender> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);
        entity.HasKey(e => e.GenderId);

        entity.ToTable("CMS_Gender", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.Property(e => e.GenderId).ValueGeneratedNever();
        entity.Property(e => e.GenderDescription)
            .HasColumnName("Gender")
            .HasMaxLength(50)
            .IsUnicode(false);
        entity.Property(e => e.NOMSId)
            .HasMaxLength(2)
            .IsUnicode(false)
            .IsFixedLength();

    }
}

