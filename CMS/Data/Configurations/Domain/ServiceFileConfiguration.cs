using CMS.Data.Constants;
using CMS.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Data.Configurations.Domain;

public class ServiceFileConfiguration : AuditableEntityConfiguration<ServiceFile>
{
    public override void Configure(EntityTypeBuilder<ServiceFile> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);

        entity.ToTable("CMS_ServiceFile", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.Property(e => e.FileCreationDate).HasColumnType("datetime");
        entity.Property(e => e.FileName)
            .HasMaxLength(256)
            .IsUnicode(false);
        entity.Property(e => e.FileTextXml).HasColumnType("xml");
        entity.Property(e => e.IsInProcess).HasDefaultValue(true);
        entity.Property(e => e.ProviderNPI)
            .HasMaxLength(10)
            .IsUnicode(false);

        entity.HasOne(d => d.Provider).WithMany(p => p.ServiceFiles)
            .HasForeignKey(d => d.ProviderId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_CMS_ServiceFile_CMS_Provider");

    }
}

