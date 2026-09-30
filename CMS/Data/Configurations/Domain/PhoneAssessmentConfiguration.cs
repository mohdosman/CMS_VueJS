using CMS.Data.Constants;
using CMS.Data.Models.Domain;
using CMS.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Data.Configurations.Domain;

public class PhoneAssessmentConfiguration : AuditableEntityConfiguration<PhoneAssessment>
{
    public override void Configure(EntityTypeBuilder<PhoneAssessment> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);
        entity.HasKey(e => e.PhoneAssessmentId);

        entity.ToTable("CMS_PhoneAssessment", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.Property(e => e.CallEnded).HasColumnType("datetime");
        entity.Property(e => e.Comment).IsUnicode(false);
        entity.Property(e => e.DispositionDispatchTime).HasColumnType("datetime");
        entity.Property(e => e.DispositionOther)
            .HasMaxLength(AssessmentFieldLimits.MaxDispositionOtherLength)
            .IsUnicode(false);
        entity.Property(e => e.ProviderPhoneAssessmentId)
            .HasMaxLength(50)
            .IsUnicode(false);

        entity.HasOne(d => d.Disposition).WithMany(p => p.PhoneAssessments)
            .HasForeignKey(d => d.DispositionId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_CMS_PhoneAssessment_CMS_Disposition");

        entity.HasOne(d => d.Patient).WithMany(p => p.PhoneAssessments)
            .HasForeignKey(d => d.PatientId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_CMS_PhoneAssessment_CMS_Patient");

        entity.HasOne(d => d.Provider).WithMany(p => p.PhoneAssessments)
            .HasForeignKey(d => d.ProviderId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_CMS_PhoneAssessment_CMS_Provider");

    }
}

