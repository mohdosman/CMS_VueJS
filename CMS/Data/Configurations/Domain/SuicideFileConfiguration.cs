using CMS.Data.Constants;
using CMS.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Data.Configurations.Domain;

public class SuicideFileConfiguration : AuditableEntityConfiguration<SuicideFile>
{
    public override void Configure(EntityTypeBuilder<SuicideFile> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);

        entity.ToTable("CMS_SuicideFile", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.Property(e => e.FileName)
            .HasMaxLength(256)
            .IsUnicode(false);

    }
}
