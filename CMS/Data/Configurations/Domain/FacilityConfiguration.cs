using CMS.Data.Constants;
using CMS.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Data.Configurations.Domain;

public class FacilityConfiguration : AuditableEntityConfiguration<Facility>
{
    public override void Configure(EntityTypeBuilder<Facility> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);
        entity.HasKey(e => e.FacilityId);

        entity.ToTable("CMS_Facility", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.Property(e => e.Abbreviation)
            .HasMaxLength(10)
            .IsUnicode(false);
        entity.Property(e => e.AddressLine1).HasMaxLength(50);
        entity.Property(e => e.AddressLine2).HasMaxLength(50);
        entity.Property(e => e.City).HasMaxLength(50);
        entity.Property(e => e.FacilityName)
            .HasMaxLength(50)
            .IsUnicode(false);
        entity.Property(e => e.State).HasMaxLength(2);
        entity.Property(e => e.ZipExtension).HasMaxLength(4);
        entity.Property(e => e.Zipcode).HasMaxLength(5);

    }
}

