using CMS.Data.Constants;
using CMS.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Data.Configurations.Domain;

public class FileUploadF2FHospAlternativeConfiguration : AuditableEntityConfiguration<FileUploadF2FHospAlternative>
{
    public override void Configure(EntityTypeBuilder<FileUploadF2FHospAlternative> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);
        entity.HasKey(e => e.F2FHospAlternativeId);

        entity.ToTable("CMS_FileUploadF2FHospAlternative", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

    }
}
