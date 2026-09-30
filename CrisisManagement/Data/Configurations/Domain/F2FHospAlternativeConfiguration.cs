using CrisisManagement.Data.Constants;
using CrisisManagement.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrisisManagement.Data.Configurations.Domain;

public class F2FHospAlternativeConfiguration : AuditableEntityConfiguration<F2FHospAlternative>
{
    public override void Configure(EntityTypeBuilder<F2FHospAlternative> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);
        entity.HasKey(e => e.F2FHospAlternativeId);

        entity.ToTable("CMS_F2FHospAlternative", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.HasOne(d => d.F2FAssessment).WithMany(p => p.F2FHospAlternatives)
            .HasForeignKey(d => d.F2FAssessmentId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_HospAlternative_F2FAssessmentId");

        entity.HasOne(d => d.HospAltDisposition).WithMany(p => p.F2FHospAlternatives)
            .HasForeignKey(d => d.HospAltDispositionId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_F2FHospAltDisposition_HospAltDispositionId");

    }
}

