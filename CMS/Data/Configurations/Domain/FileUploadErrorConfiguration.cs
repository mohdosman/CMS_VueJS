using CMS.Data.Constants;
using CMS.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Data.Configurations.Domain;

public class FileUploadErrorConfiguration : AuditableEntityConfiguration<FileUploadError>
{
    public override void Configure(EntityTypeBuilder<FileUploadError> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);
        entity.HasKey(e => e.FileUploadErrorId);

        entity.ToTable("CMS_FileUploadError", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.HasOne(d => d.FileUploadErrorCode).WithMany(p => p.FileUploadErrors)
            .HasForeignKey(d => d.FileUploadErrorCodeId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_CMS_FileUploadError_CMS_FileUploadErrorCode");

        entity.HasOne(d => d.FileUploadErrorTable).WithMany(p => p.FileUploadErrors)
            .HasForeignKey(d => d.FileUploadErrorTableId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_CMS_FileUploadError_CMS_FileUploadErrorTable");

        entity.HasOne(d => d.FileUpload).WithMany(p => p.FileUploadErrors)
            .HasForeignKey(d => d.FileUploadId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_CMS_FileUploadError_CMS_FileUpload");

    }
}

