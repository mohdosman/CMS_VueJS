using CMS.Data.Constants;
using CMS.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Data.Configurations.Domain;

public class F2FEvaluationConfiguration : AuditableEntityConfiguration<F2FEvaluation>
{
    public override void Configure(EntityTypeBuilder<F2FEvaluation> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);

        entity.ToTable("CMS_F2FEvaluation", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.HasOne(d => d.EvaluationType).WithMany(p => p.F2FEvaluations)
            .HasForeignKey(d => d.EvaluationTypeId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_CMS_F2FEvaluation_CMS_EvaluationType");

        entity.HasOne(d => d.F2FAssessment).WithMany(p => p.F2FEvaluations)
            .HasForeignKey(d => d.F2FAssessmentId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_CMS_F2FEvaluation_CMS_F2FAssessment");

    }
}

