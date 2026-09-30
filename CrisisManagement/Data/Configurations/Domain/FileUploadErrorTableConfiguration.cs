using CrisisManagement.Data.Constants;
using CrisisManagement.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrisisManagement.Data.Configurations.Domain;

public class FileUploadErrorTableConfiguration : AuditableEntityConfiguration<FileUploadErrorTable>
{
    public override void Configure(EntityTypeBuilder<FileUploadErrorTable> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);
        entity.HasKey(e => e.FileUploadErrorTableId);

        entity.ToTable("CMS_FileUploadErrorTable", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.Property(e => e.FileUploadErrorTableId).ValueGeneratedNever();
        entity.Property(e => e.Abbrev)
            .HasMaxLength(50)
            .IsUnicode(false);
        entity.Property(e => e.FileUploadErrorTableDescription)
            .HasColumnName("FileUploadErrorTable")
            .HasMaxLength(500)
            .IsUnicode(false);

    }
}

