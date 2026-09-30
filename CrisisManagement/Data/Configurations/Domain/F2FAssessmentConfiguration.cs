using CrisisManagement.Data.Constants;
using CrisisManagement.Data.Models.Domain;
using CrisisManagement.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrisisManagement.Data.Configurations.Domain;

public class F2FAssessmentConfiguration : AuditableEntityConfiguration<F2FAssessment>
{
    public override void Configure(EntityTypeBuilder<F2FAssessment> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);
        entity.HasKey(e => e.F2FAssessmentId);

        entity.ToTable("CMS_F2FAssessment", DatabaseConstants.DefaultSchema, t =>
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
        entity.Property(e => e.ProviderF2FAssessmentId)
            .HasMaxLength(50)
            .IsUnicode(false);
        entity.Property(e => e.TimeDispositionCompleted).HasColumnType("datetime");
        entity.Property(e => e.TimeTransported).HasColumnType("datetime");

        entity.HasOne(d => d.AssessmentLocationt).WithMany(p => p.F2FAssessments)
            .HasForeignKey(d => d.AssessmentLocationtId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_CMS_F2FAssessment_CMS_AssessmentLocation");

        entity.HasOne(d => d.AssessmentType).WithMany(p => p.F2FAssessments)
            .HasForeignKey(d => d.AssessmentTypeId)
            .HasConstraintName("FK_CMS_F2FAssessment_CMS_AssessmentType");

        entity.HasOne(d => d.County).WithMany(p => p.F2FAssessments)
            .HasForeignKey(d => d.CountyId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_F2FAssessment_CountyId");

        entity.HasOne(d => d.CurrentServices).WithMany(p => p.F2FAssessments)
            .HasForeignKey(d => d.CurrentServicesId)
            .HasConstraintName("FK_CMS_F2FAssessment_CMS_CurrentServices");

        entity.HasOne(d => d.DurablePOA).WithMany(p => p.F2FAssessmentDurablePOAs)
            .HasForeignKey(d => d.DurablePOAId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_F2FAssessment_DurablePOA");

        entity.HasOne(d => d.EducationLevel).WithMany(p => p.F2FAssessments)
            .HasForeignKey(d => d.EducationLevelId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_F2FAssessment_EducationLevelId");

        entity.HasOne(d => d.EmploymentStatus).WithMany(p => p.F2FAssessments)
            .HasForeignKey(d => d.EmploymentStatusId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_F2FAssessment_EmploymentStatusID");

        entity.HasOne(d => d.FirstHospitalization).WithMany(p => p.F2FAssessmentFirstHospitalizations)
            .HasForeignKey(d => d.FirstHospitalizationId)
            .HasConstraintName("FK_CMS_F2FAssessment_CMS_YesNoUnknown");

        entity.HasOne(d => d.IntellectualDisability).WithMany(p => p.F2FAssessmentIntellectualDisabilities)
            .HasForeignKey(d => d.IntellectualDisabilityId)
            .HasConstraintName("FK_CMS_F2FAssessment_CMS_YesNoUnknownID");

        entity.HasOne(d => d.MHTreatmentDeclaration).WithMany(p => p.F2FAssessmentMHTreatmentDeclarations)
            .HasForeignKey(d => d.MHTreatmentDeclarationId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_F2FAssessment_MHTreatmentDeclaration");

        entity.HasOne(d => d.MOTStatus).WithMany(p => p.F2FAssessmentMOTStatuses)
            .HasForeignKey(d => d.MOTStatusId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_F2FAssessment_MOTStatus");

        entity.HasOne(d => d.MaritalStatus).WithMany(p => p.F2FAssessments)
            .HasForeignKey(d => d.MaritalStatusId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_F2FAssessment_MaritalStatusID");

        entity.HasOne(d => d.MedicalInstability).WithMany(p => p.F2FAssessmentMedicalInstabilities)
            .HasForeignKey(d => d.MedicalInstabilityId)
            .HasConstraintName("FK_CMS_F2FAssessment_CMS_YesNoUnknownMI");

        entity.HasOne(d => d.MedicationIssues).WithMany(p => p.F2FAssessmentMedicationIssues)
            .HasForeignKey(d => d.MedicationIssuesId)
            .HasConstraintName("FK_CMS_F2FAssessment_CMS_YesNoUnknownMS");

        entity.HasOne(d => d.MilitaryStatus).WithMany(p => p.F2FAssessments)
            .HasForeignKey(d => d.MilitaryStatusId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_F2FAssessment_MilitaryStatusID");

        entity.HasOne(d => d.PastTrauma).WithMany(p => p.F2FAssessmentPastTraumas)
            .HasForeignKey(d => d.PastTraumaId)
            .HasConstraintName("FK_CMS_F2FAssessment_CMS_YesNoUnknownPT");

        entity.HasOne(d => d.Patient).WithMany(p => p.F2FAssessments)
            .HasForeignKey(d => d.PatientId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_F2FAssessment_PatientID");

        entity.HasOne(d => d.PayorSource).WithMany(p => p.F2FAssessmentPayorSources)
            .HasForeignKey(d => d.PayorSourceId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_F2FAssessment_PayorSourceId");

        entity.HasOne(d => d.PhoneAssessment).WithMany(p => p.F2FAssessments)
            .HasForeignKey(d => d.PhoneAssessmentId)
            .HasConstraintName("FK_F2FAssessment_PhoneAssessmentId");

        entity.HasOne(d => d.PrimaryProblem).WithMany(p => p.F2FAssessments)
            .HasForeignKey(d => d.PrimaryProblemId)
            .HasConstraintName("FK_CMS_F2FAssessment_CMS_PrimaryProblem");

        entity.HasOne(d => d.Provider).WithMany(p => p.F2FAssessments)
            .HasForeignKey(d => d.ProviderId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_F2FAssessment_ProviderId");

        entity.HasOne(d => d.RecommendedTransportMode).WithMany(p => p.F2FAssessments)
            .HasForeignKey(d => d.RecommendedTransportModeId)
            .HasConstraintName("FK_RecommendedTransportMode_RecommendedTransportModeId");

        entity.HasOne(d => d.ResidentialStatus).WithMany(p => p.F2FAssessments)
            .HasForeignKey(d => d.ResidentialStatusId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_F2FAssessment_ResidentialStatusID");

        entity.HasOne(d => d.School3Months).WithMany(p => p.F2FAssessmentSchool3Months)
            .HasForeignKey(d => d.School3MonthsId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_F2FAssessment_School3Months");

        entity.HasOne(d => d.SecondaryPayorSource).WithMany(p => p.F2FAssessmentSecondaryPayorSources)
            .HasForeignKey(d => d.SecondaryPayorSourceId)
            .HasConstraintName("FK_CMS_F2FAssessment_CMS_PayorSource");

        entity.HasOne(d => d.SubstanceAbuse).WithMany(p => p.F2FAssessmentSubstanceAbuses)
            .HasForeignKey(d => d.SubstanceAbuseId)
            .HasConstraintName("FK_CMS_F2FAssessment_CMS_YesNoUnknownSA");

    }
}

