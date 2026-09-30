using CMS.Data.Constants;
using CMS.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Data.Configurations.Domain;

public class EvaluationTypeConfiguration : AuditableEntityConfiguration<EvaluationType>
{
    public override void Configure(EntityTypeBuilder<EvaluationType> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);

        entity.ToTable("CMS_EvaluationType", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.Property(e => e.EvaluationTypeDescription)
            .HasColumnName("EvaluationType")
            .HasMaxLength(500)
            .IsUnicode(false);

        entity.HasOne(d => d.EvaluationCategory).WithMany(p => p.EvaluationTypes)
            .HasForeignKey(d => d.EvaluationCategoryId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_EvaluationType_EvaluationCategoryId");

    }
}

