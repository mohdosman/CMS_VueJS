using CrisisManagement.Data.Constants;
using CrisisManagement.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrisisManagement.Data.Configurations.Domain;

public class CountyConfiguration : AuditableEntityConfiguration<County>
{
    public override void Configure(EntityTypeBuilder<County> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);
        entity.HasKey(e => e.CountyId);

        entity.ToTable("CMS_County", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.Property(e => e.CountyDescription)
            .HasColumnName("County")
            .HasMaxLength(50)
            .IsUnicode(false);
        entity.Property(e => e.CountyCode)
            .HasMaxLength(50)
            .IsUnicode(false);
        entity.Property(e => e.MHPlanningRegion).HasDefaultValue((byte)0);
        entity.Property(e => e.ServiceArea)
            .HasMaxLength(50)
            .IsUnicode(false);

    }
}

