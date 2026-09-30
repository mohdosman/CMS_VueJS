using CMS.Data.Constants;
using CMS.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Data.Configurations.Domain;

public class F2FHospitalizationConfiguration : AuditableEntityConfiguration<F2FHospitalization>
{
    public override void Configure(EntityTypeBuilder<F2FHospitalization> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);
        entity.HasKey(e => e.F2FHospitalizationId);

        entity.ToTable("CMS_F2FHospitalization", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.HasOne(d => d.F2FAssessment).WithMany(p => p.F2FHospitalizations)
            .HasForeignKey(d => d.F2FAssessmentId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_Hospitalization_F2FAssessmentId");

        entity.HasOne(d => d.HospitalizationDisposition).WithMany(p => p.F2FHospitalizations)
            .HasForeignKey(d => d.HospitalizationDispositionId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_F2FHospitalization_HospitalizationDispositionId");

        entity.HasOne(d => d.Hospitalization).WithMany(p => p.F2FHospitalizations)
            .HasForeignKey(d => d.HospitalizationId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_F2FHospitalization_HospitalizationId");

    }
}

