using CrisisManagement.Data.Constants;
using CrisisManagement.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrisisManagement.Data.Configurations.Domain;

public class YesNoUnknownConfiguration : AuditableEntityConfiguration<YesNoUnknown>
{
    public override void Configure(EntityTypeBuilder<YesNoUnknown> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);
        entity.HasKey(e => e.YesNoUnknownId);

        entity.ToTable("CMS_YesNoUnknown", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.Property(e => e.YesNoUnknownId).ValueGeneratedNever();
        entity.Property(e => e.Abbreviation)
            .HasMaxLength(2)
            .IsUnicode(false);
        entity.Property(e => e.Response)
            .HasMaxLength(10)
            .IsUnicode(false);

    }
}
