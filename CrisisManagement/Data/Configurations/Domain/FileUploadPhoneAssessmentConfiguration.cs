using CrisisManagement.Data.Constants;
using CrisisManagement.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrisisManagement.Data.Configurations.Domain;

public class FileUploadPhoneAssessmentConfiguration : AuditableEntityConfiguration<FileUploadPhoneAssessment>
{
    public override void Configure(EntityTypeBuilder<FileUploadPhoneAssessment> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);
        entity.HasKey(e => e.PhoneAssessmentId);

        entity.ToTable("CMS_FileUploadPhoneAssessment", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.Property(e => e.CallEnded).HasColumnType("datetime");
        entity.Property(e => e.Comment).IsUnicode(false);
        entity.Property(e => e.DispositionDispatchTime).HasColumnType("datetime");
        entity.Property(e => e.DispositionOther)
            .HasMaxLength(150)
            .IsUnicode(false);
        entity.Property(e => e.F2FAssessmentXML).HasColumnType("xml");
        entity.Property(e => e.ProviderPhoneAssessmentId)
            .HasMaxLength(50)
            .IsUnicode(false);

    }
}
