using CMS.Data.Constants;
using CMS.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Data.Configurations.Domain;

public class DepartmentConfiguration : AuditableEntityConfiguration<Department>
{
    public override void Configure(EntityTypeBuilder<Department> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);
        entity.HasKey(e => e.DepartmentId);

        entity.ToTable("CMS_Department", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.Property(e => e.AddressLine1)
            .HasMaxLength(50)
            .IsUnicode(false);
        entity.Property(e => e.AddressLine2)
            .HasMaxLength(50)
            .IsUnicode(false);
        entity.Property(e => e.City)
            .HasMaxLength(50)
            .IsUnicode(false);
        entity.Property(e => e.DepartmentName)
            .HasMaxLength(256)
            .IsUnicode(false);
        entity.Property(e => e.DivisionName)
            .HasMaxLength(256)
            .IsUnicode(false);
        entity.Property(e => e.Notes).IsUnicode(false);
        entity.Property(e => e.State)
            .HasMaxLength(50)
            .IsUnicode(false);
        entity.Property(e => e.Zip)
            .HasMaxLength(5)
            .IsUnicode(false)
            .IsFixedLength();
        entity.Property(e => e.ZipExtension)
            .HasMaxLength(4)
            .IsUnicode(false)
            .IsFixedLength();

    }
}

