using CMS.Data.Constants;
using CMS.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Data.Configurations.Domain;

public class ServiceFileErrorCodeConfiguration : AuditableEntityConfiguration<ServiceFileErrorCode>
{
    public override void Configure(EntityTypeBuilder<ServiceFileErrorCode> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);
        entity.HasKey(e => e.ServiceFileErrorCodeId);

        entity.ToTable("CMS_ServiceFileErrorCode", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.Property(e => e.ServiceFileErrorCodeDescription)
            .HasMaxLength(500)
            .IsUnicode(false);

    }
}
