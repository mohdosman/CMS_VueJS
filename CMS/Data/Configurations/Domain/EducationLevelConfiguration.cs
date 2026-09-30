using CMS.Data.Constants;
using CMS.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Data.Configurations.Domain;

public class EducationLevelConfiguration : AuditableEntityConfiguration<EducationLevel>
{
    public override void Configure(EntityTypeBuilder<EducationLevel> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);

        entity.ToTable("CMS_EducationLevel", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.Property(e => e.EducationLevelDescription)
            .HasColumnName("EducationLevel")
            .HasMaxLength(150)
            .IsUnicode(false);
        entity.Property(e => e.NOMSId)
            .HasMaxLength(2)
            .IsUnicode(false)
            .IsFixedLength();

    }
}

