using CMS.Data.Constants;
using CMS.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Data.Configurations.Domain;

public class FileUploadF2FHospitalizationConfiguration : AuditableEntityConfiguration<FileUploadF2FHospitalization>
{
    public override void Configure(EntityTypeBuilder<FileUploadF2FHospitalization> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);
        entity.HasKey(e => e.F2FHospitalizationId);

        entity.ToTable("CMS_FileUploadF2FHospitalization", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

    }
}
