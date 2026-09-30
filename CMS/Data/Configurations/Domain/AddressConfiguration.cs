using CMS.Data.Constants;
using CMS.Data.Models.Domain;
using CMS.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Data.Configurations.Domain;

public class AddressConfiguration : AuditableEntityConfiguration<Address>
{
    public override void Configure(EntityTypeBuilder<Address> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);
        entity.HasKey(e => e.AddressId);

        entity.ToTable("CMS_Address", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.Property(e => e.AddressLine1)
            .HasMaxLength(ProviderFieldLimits.MaxAddressLineLength)
            .IsUnicode(false);
        entity.Property(e => e.AddressLine2)
            .HasMaxLength(ProviderFieldLimits.MaxAddressLineLength)
            .IsUnicode(false);
        entity.Property(e => e.City)
            .HasMaxLength(ProviderFieldLimits.MaxAddressLineLength)
            .IsUnicode(false);
        entity.Property(e => e.IsActive).HasDefaultValue(true);
        entity.Property(e => e.ZipExtension)
            .HasMaxLength(ProviderFieldLimits.MaxZipExtensionLength)
            .IsUnicode(false);
        entity.Property(e => e.Zipcode)
            .HasMaxLength(ProviderFieldLimits.MaxZipcodeLength)
            .IsUnicode(false);

        entity.HasOne(d => d.Contact).WithMany(p => p.Addresses)
            .HasForeignKey(d => d.ContactId)
            .HasConstraintName("FK_Address_Contact");

        entity.HasOne(d => d.County).WithMany(p => p.Addresses)
            .HasForeignKey(d => d.CountyId)
            .HasConstraintName("FK_Address_CountyID");

        entity.HasOne(d => d.State).WithMany(p => p.Addresses)
            .HasForeignKey(d => d.StateId)
            .HasConstraintName("FK_Address_StateID");

    }
}

