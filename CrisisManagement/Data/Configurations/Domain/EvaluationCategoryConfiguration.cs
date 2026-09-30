using CrisisManagement.Data.Constants;
using CrisisManagement.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrisisManagement.Data.Configurations.Domain;

public class EvaluationCategoryConfiguration : AuditableEntityConfiguration<EvaluationCategory>
{
    public override void Configure(EntityTypeBuilder<EvaluationCategory> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);

        entity.ToTable("CMS_EvaluationCategory", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.Property(e => e.EvaluationCategoryDescription)
            .HasColumnName("EvaluationCategory")
            .HasMaxLength(500)
            .IsUnicode(false);

    }
}

