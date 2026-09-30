using CMS.Data.Constants;
using CMS.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Data.Configurations.Domain;

public class PatientAddressConfiguration : AuditableEntityConfiguration<PatientAddress>
{
    public override void Configure(EntityTypeBuilder<PatientAddress> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);

        entity.ToTable("CMS_PatientAddress", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.HasOne(d => d.Address).WithMany(p => p.PatientAddresses)
            .HasForeignKey(d => d.AddressId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_CMS_PatientAddress_CMS_Address");

        entity.HasOne(d => d.AddressType).WithMany(p => p.PatientAddresses)
            .HasForeignKey(d => d.AddressTypeId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_CMS_PatientAddress_CMS_AddressType");

        entity.HasOne(d => d.Patient).WithMany(p => p.PatientAddresses)
            .HasForeignKey(d => d.PatientId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_CMS_PatientAddress_CMS_Patient");

    }
}

