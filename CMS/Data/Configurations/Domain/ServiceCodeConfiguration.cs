using CMS.Data.Constants;
using CMS.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Data.Configurations.Domain;

public class ServiceCodeConfiguration : AuditableEntityConfiguration<ServiceCode>
{
    public override void Configure(EntityTypeBuilder<ServiceCode> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);

        entity.ToTable("CMS_ServiceCode", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.Property(e => e.ServiceCodeAbbrev)
            .HasMaxLength(50)
            .IsUnicode(false);
        entity.Property(e => e.ServiceCodeName)
            .HasMaxLength(255)
            .IsUnicode(false);

    }
}
