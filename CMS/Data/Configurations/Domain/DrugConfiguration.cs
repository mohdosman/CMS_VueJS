using CMS.Data.Constants;
using CMS.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Data.Configurations.Domain;

public class DrugConfiguration : AuditableEntityConfiguration<Drug>
{
    public override void Configure(EntityTypeBuilder<Drug> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);
        entity.HasKey(e => e.DrugId);

        entity.ToTable("CMS_Drug", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.Property(e => e.DrugDescription)
            .HasColumnName("Drug")
            .HasMaxLength(150)
            .IsUnicode(false);
        entity.Property(e => e.NOMSId)
            .HasMaxLength(2)
            .IsUnicode(false)
            .IsFixedLength();

    }
}

