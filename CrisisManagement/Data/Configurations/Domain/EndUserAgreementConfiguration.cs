using CrisisManagement.Data.Constants;
using CrisisManagement.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrisisManagement.Data.Configurations.Domain;

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
