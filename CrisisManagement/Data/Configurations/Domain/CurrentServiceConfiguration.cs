using CrisisManagement.Data.Constants;
using CrisisManagement.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrisisManagement.Data.Configurations.Domain;

public class CurrentServiceConfiguration : AuditableEntityConfiguration<CurrentService>
{
    public override void Configure(EntityTypeBuilder<CurrentService> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);
        entity.HasKey(e => e.CurrentServicesId);

        entity.ToTable("CMS_CurrentServices", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.Property(e => e.CurrentServices)
            .HasMaxLength(120)
            .IsUnicode(false);

    }
}
