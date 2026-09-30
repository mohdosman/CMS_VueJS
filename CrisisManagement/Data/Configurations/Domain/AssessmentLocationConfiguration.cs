using CrisisManagement.Data.Constants;
using CrisisManagement.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrisisManagement.Data.Configurations.Domain;

public class AssessmentLocationConfiguration : AuditableEntityConfiguration<AssessmentLocation>
{
    public override void Configure(EntityTypeBuilder<AssessmentLocation> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);

        entity.ToTable("CMS_AssessmentLocation", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.Property(e => e.AssessmentLocationDescription)
            .HasColumnName("AssessmentLocation")
            .HasMaxLength(150)
            .IsUnicode(false);

    }
}

