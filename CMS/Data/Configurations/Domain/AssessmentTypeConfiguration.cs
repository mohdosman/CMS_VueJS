using CMS.Data.Constants;
using CMS.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Data.Configurations.Domain;

public class AssessmentTypeConfiguration : AuditableEntityConfiguration<AssessmentType>
{
    public override void Configure(EntityTypeBuilder<AssessmentType> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);

        entity.ToTable("CMS_AssessmentType", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.Property(e => e.AssessmentTypeDescription)
            .HasColumnName("AssessmentType")
            .HasMaxLength(50)
            .IsUnicode(false);

    }
}

