using CrisisManagement.Data.Constants;
using CrisisManagement.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrisisManagement.Data.Configurations.Domain;

public class DrugFrequencyConfiguration : AuditableEntityConfiguration<DrugFrequency>
{
    public override void Configure(EntityTypeBuilder<DrugFrequency> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);
        entity.HasKey(e => e.DrugFrequencyId);

        entity.ToTable("CMS_DrugFrequency", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.Property(e => e.DrugFrequencyDescription)
            .HasColumnName("DrugFrequency")
            .HasMaxLength(150)
            .IsUnicode(false);
        entity.Property(e => e.NOMSId)
            .HasMaxLength(2)
            .IsUnicode(false)
            .IsFixedLength();

    }
}

