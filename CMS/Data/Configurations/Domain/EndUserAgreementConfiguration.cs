using CMS.Data.Constants;
using CMS.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Data.Configurations.Domain;

public class EndUserAgreementConfiguration : AuditableEntityConfiguration<EndUserAgreement>
{
    public override void Configure(EntityTypeBuilder<EndUserAgreement> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);

        entity.ToTable("CMS_EndUserAgreement", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.Property(e => e.SessionId).HasMaxLength(256);

    }
}
