using CMS.Data.Constants;
using CMS.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Data.Configurations.Domain;

public class AddressTypeConfiguration : AuditableEntityConfiguration<AddressType>
{
    public override void Configure(EntityTypeBuilder<AddressType> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);

        entity.ToTable("CMS_AddressType", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.Property(e => e.AddressTypeDescription)
            .HasColumnName("AddressType")
            .HasMaxLength(50)
            .IsUnicode(false);

    }
}

