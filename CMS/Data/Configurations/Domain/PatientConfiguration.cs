using CMS.Data.Constants;
using CMS.Data.Models.Domain;
using CMS.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Data.Configurations.Domain;

public sealed class PatientConfiguration : AuditableEntityConfiguration<Patient>
{
    public override void Configure(EntityTypeBuilder<Patient> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);

        entity.HasKey(e => e.PatientId);

        entity.ToTable("CMS_Patient", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.HasIndex(e => e.SSN, "IX_SSN");

        entity.Property(e => e.DOB).HasColumnType("datetime");
        entity.Property(e => e.EthnicityId).HasDefaultValue((byte)4);
        entity.Property(e => e.FirstName)
            .HasMaxLength(PatientFieldLimits.MaxNameLength)
            .IsUnicode(false);
        entity.Property(e => e.GenderId).HasDefaultValue((byte)5);
        entity.Property(e => e.LastName)
            .HasMaxLength(PatientFieldLimits.MaxNameLength)
            .IsUnicode(false);
        entity.Property(e => e.ProviderPatientNo)
            .HasMaxLength(PatientFieldLimits.MaxProviderPatientNoLength)
            .IsUnicode(false);
        entity.Property(e => e.RaceId).HasDefaultValue((byte)8);
        entity.Property(e => e.SSN)
            .HasMaxLength(9)
            .IsUnicode(false)
            .IsFixedLength();

        entity.HasOne(d => d.Ethnicity).WithMany(p => p.Patients)
            .HasForeignKey(d => d.EthnicityId)
            .HasConstraintName("FK_Patient_EthnicityID");

        entity.HasOne(d => d.Gender).WithMany(p => p.Patients)
            .HasForeignKey(d => d.GenderId)
            .HasConstraintName("FK_Patient_GenderID");

        entity.HasOne(d => d.Race).WithMany(p => p.Patients)
            .HasForeignKey(d => d.RaceId)
            .HasConstraintName("FK_Patient_RaceID");

    }
}
