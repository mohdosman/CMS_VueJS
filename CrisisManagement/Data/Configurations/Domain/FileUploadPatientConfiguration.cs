using CrisisManagement.Data.Constants;
using CrisisManagement.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrisisManagement.Data.Configurations.Domain;

public class FileUploadPatientConfiguration : AuditableEntityConfiguration<FileUploadPatient>
{
    public override void Configure(EntityTypeBuilder<FileUploadPatient> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);
        entity.HasKey(e => e.PatientId);

        entity.ToTable("CMS_FileUploadPatient", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.Property(e => e.AssessmentXML).HasColumnType("xml");
        entity.Property(e => e.DOB).HasColumnType("datetime");
        entity.Property(e => e.FirstName)
            .HasMaxLength(25)
            .IsUnicode(false);
        entity.Property(e => e.LastName)
            .HasMaxLength(25)
            .IsUnicode(false);
        entity.Property(e => e.ProviderPatientNo)
            .HasMaxLength(50)
            .IsUnicode(false);
        entity.Property(e => e.SSN)
            .HasMaxLength(9)
            .IsUnicode(false)
            .IsFixedLength();

    }
}
