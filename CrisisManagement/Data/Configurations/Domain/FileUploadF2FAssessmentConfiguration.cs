using CrisisManagement.Data.Constants;
using CrisisManagement.Data.Models.Domain;
using CrisisManagement.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrisisManagement.Data.Configurations.Domain;

public class FileUploadF2FAssessmentConfiguration : AuditableEntityConfiguration<FileUploadF2FAssessment>
{
    public override void Configure(EntityTypeBuilder<FileUploadF2FAssessment> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);
        entity.HasKey(e => e.F2FAssessmentId);

        entity.ToTable("CMS_FileUploadF2FAssessment", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.Property(e => e.AnnualHouseholdIncome).HasColumnType("money");
        entity.Property(e => e.CompletedByFirstName)
            .HasMaxLength(AssessmentFieldLimits.MaxCompletedByNameLength)
            .IsUnicode(false);
        entity.Property(e => e.CompletedByLastName)
            .HasMaxLength(AssessmentFieldLimits.MaxCompletedByNameLength)
            .IsUnicode(false);
        entity.Property(e => e.F2FAssessmentDate).HasColumnType("datetime");
        entity.Property(e => e.F2FAssessmentXML).HasColumnType("xml");
        entity.Property(e => e.PrimaryMHSADiagnosis)
            .HasMaxLength(25)
            .IsUnicode(false);
        entity.Property(e => e.ProviderF2FAssessmentId)
            .HasMaxLength(50)
            .IsUnicode(false);
        entity.Property(e => e.SecondaryMHSADiagnosis)
            .HasMaxLength(25)
            .IsUnicode(false);
        entity.Property(e => e.TimeDispositionCompleted).HasColumnType("datetime");
        entity.Property(e => e.TimeTransported).HasColumnType("datetime");

    }
}
