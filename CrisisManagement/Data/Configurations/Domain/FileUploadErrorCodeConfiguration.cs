using CrisisManagement.Data.Constants;
using CrisisManagement.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrisisManagement.Data.Configurations.Domain;

public class FileUploadErrorCodeConfiguration : AuditableEntityConfiguration<FileUploadErrorCode>
{
    public override void Configure(EntityTypeBuilder<FileUploadErrorCode> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);
        entity.HasKey(e => e.FileUploadErrorCodeId);

        entity.ToTable("CMS_FileUploadErrorCode", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.Property(e => e.FileUploadErrorCodeDescription)
            .HasMaxLength(500)
            .IsUnicode(false);

    }
}
