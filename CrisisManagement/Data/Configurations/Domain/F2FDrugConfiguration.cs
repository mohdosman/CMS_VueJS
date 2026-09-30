using CrisisManagement.Data.Constants;
using CrisisManagement.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrisisManagement.Data.Configurations.Domain;

public class F2FDrugConfiguration : AuditableEntityConfiguration<F2FDrug>
{
    public override void Configure(EntityTypeBuilder<F2FDrug> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);
        entity.HasKey(e => e.F2FDrugId);

        entity.ToTable("CMS_F2FDrug", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.HasOne(d => d.DrugFrequency).WithMany(p => p.F2FDrugs)
            .HasForeignKey(d => d.DrugFrequencyId)
            .HasConstraintName("FK_F2FDrug_DrugFrequencyId");

        entity.HasOne(d => d.Drug).WithMany(p => p.F2FDrugs)
            .HasForeignKey(d => d.DrugId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_F2FDrug_DrugId");

        entity.HasOne(d => d.DrugRoute).WithMany(p => p.F2FDrugs)
            .HasForeignKey(d => d.DrugRouteId)
            .HasConstraintName("FK_F2FDrug_DrugRouteId");

        entity.HasOne(d => d.F2FAssessment).WithMany(p => p.F2FDrugs)
            .HasForeignKey(d => d.F2FAssessmentId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_Drug_F2FAssessmentId");

    }
}

