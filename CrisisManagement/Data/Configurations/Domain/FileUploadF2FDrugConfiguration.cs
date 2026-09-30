using CrisisManagement.Data.Constants;
using CrisisManagement.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrisisManagement.Data.Configurations.Domain;

public class FileUploadF2FDrugConfiguration : AuditableEntityConfiguration<FileUploadF2FDrug>
{
    public override void Configure(EntityTypeBuilder<FileUploadF2FDrug> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);
        entity.HasKey(e => e.F2FDrugId);

        entity.ToTable("CMS_FileUploadF2FDrug", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

    }
}
