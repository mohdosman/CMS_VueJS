using CrisisManagement.Data.Constants;
using CrisisManagement.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrisisManagement.Data.Configurations.Domain;

public class ProviderAddressConfiguration : AuditableEntityConfiguration<ProviderAddress>
{
    public override void Configure(EntityTypeBuilder<ProviderAddress> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);

        entity.ToTable("CMS_ProviderAddress", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.HasOne(d => d.Address).WithMany(p => p.ProviderAddresses)
            .HasForeignKey(d => d.AddressId)
            .HasConstraintName("FK_CMS_ProviderAddress_CMS_Address");

        entity.HasOne(d => d.AddressType).WithMany(p => p.ProviderAddresses)
            .HasForeignKey(d => d.AddressTypeId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_CMS_ProviderAddress_CMS_AddressType");

        entity.HasOne(d => d.Provider).WithMany(p => p.ProviderAddresses)
            .HasForeignKey(d => d.ProviderId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_CMS_ProviderAddress_CMS_Provider");

    }
}

