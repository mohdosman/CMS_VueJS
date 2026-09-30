using CMS.Data.Constants;
using CMS.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Data.Configurations.Domain;

public class ServiceFileErrorConfiguration : AuditableEntityConfiguration<ServiceFileError>
{
    public override void Configure(EntityTypeBuilder<ServiceFileError> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);
        entity.HasKey(e => e.ServiceFileErrorId);

        entity.ToTable("CMS_ServiceFileError", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.HasOne(d => d.ServiceFileErrorCode).WithMany(p => p.ServiceFileErrors)
            .HasForeignKey(d => d.ServiceFileErrorCodeId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_CMS_ServiceFileError_CMS_ServiceFileErrorCode");

        entity.HasOne(d => d.ServiceFileImport).WithMany(p => p.ServiceFileErrors)
            .HasForeignKey(d => d.ServiceFileImportId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_CMS_ServiceFileError_CMS_ServiceFileImport");

    }
}

